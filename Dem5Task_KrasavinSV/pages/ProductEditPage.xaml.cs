using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dem5Task_KrasavinSV.db;
using Microsoft.Win32;

namespace Dem5Task_KrasavinSV.pages
{
    /// <summary>
    /// Логика взаимодействия для ProductEditPage.xaml
    /// </summary>
    public partial class ProductEditPage : Page
    {
        Product product;
        public ProductEditPage(Product product)
        {
            this.product = product;
            DataContext = product;
            InitializeComponent();
            UpdateMaterialList();
            CbProductTypes.ItemsSource = EfModel.Init().ProductTypes.ToList();
        }

        private void UpdateMaterialList()
        {
            DgvMaterialsList.ItemsSource = product.ProductMaterials
                .Where(pm => pm.Material.Title.Contains(TbMaterialSeatch.Text)).ToList();
            DgvMaterialsList.Items.Refresh();

            CbMaterials.ItemsSource = EfModel.Init().Materials.ToList()
                .Where(m => !product.ProductMaterials.Select(pm => pm.Material).Contains(m));
        }

        private void ImageChangeClick(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog { Filter = "Jpeg files|*.jpg|All Files|*.*" };
            openFile.ShowDialog();
            if(openFile.ShowDialog() == true)
            {
                product.Image = File.ReadAllBytes(openFile.FileName);
            }
        }

        private void SaveChanges(object sender, RoutedEventArgs e)
        {
            if (EfModel.Init().Products.FirstOrDefault(p => p.ArticleNumber == product.ArticleNumber) != null)
            {
                //Выдаем ошибку
                MessageBox.Show("Артикул уже используется другим товаром!");
                return;
            }
            try
            {
                if (product.ID == 0)
                {
                    EfModel.Init().Products.Add(product);
                    EfModel.Init().SaveChanges();

                    NavigationService.Navigate(new ProductList());

                    MessageBox.Show("Продукт \"" + product.Title + "\" успешно создан!");
                }
                else
                {
                    NavigationService.Navigate(new ProductList());
                    EfModel.Init().SaveChanges();

                    MessageBox.Show("Редактирование прошло успешно");
                }
                    
            } catch(DbEntityValidationException ex)
            {
                MessageBox.Show(String.Join(Environment.NewLine, ex.EntityValidationErrors.Last().ValidationErrors.Select(ve => ve.ErrorMessage)));
            }
        }

        private void DestroyProduct(object sender, RoutedEventArgs e)
        {
            if (product.ProductSales.Count > 0)
            {
                MessageBox.Show("Удаление товара запрещено, товар продавался");
                return;
            }
            else
            {
                try
                {
                    if (MessageBox.Show("Вы действительно хотите удалить товар: \"" + product.Title + "\"?", "Удалить товар?",
                       MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        string summary = product.Title;
                        product.ProductMaterials.Clear();
                        product.ProductCostHistories.Clear();
                        Product isDestroed = product;
                        EfModel.Init().Products.Remove(product);
                        EfModel.Init().SaveChanges();
                        //isDestroed.ID = 0;
                        if (NavigationService.CanGoBack)
                        {
                            NavigationService.GoBack();
                        }
                        //NavigationService.Navigate(new ProductList());
                        MessageBox.Show("Товар: \"" + summary + "\" и зависимые от него данные были успешно удалены");
                    }
                } catch (Exception)
                {
                    MessageBox.Show("Удаление не возможно, объект уже был удалён");
                }
            }
        }

        private void MaterialAddClick(object sender, RoutedEventArgs e)
        {
            int count;

            if (CbMaterials.SelectedItem != null
                && TbCount.Text.Length > 0
                && Int32.TryParse(TbCount.Text, out count)
            )
            {

                product.ProductMaterials.Add(

                    new ProductMaterial { Material = (CbMaterials.SelectedItem as Material), Count = count }
                );
                DgvMaterialsList.Items.Refresh();

                CbMaterials.ItemsSource = EfModel.Init().Materials.ToList()
                    .Where(m => !product.ProductMaterials.Select(pm => pm.Material).Contains(m));

                UpdateMaterialList();
            }
            else
            {

                MessageBox.Show("Что-то пошло не так! Проверьте данные");
            }
        }

        private void DestroyMaterialClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            product.ProductMaterials.Remove(button.DataContext as ProductMaterial);

            UpdateMaterialList();
        }

        private void MaterialSeatchEvent(object sender, TextChangedEventArgs e)
        {
            UpdateMaterialList();
        }
    }
}
