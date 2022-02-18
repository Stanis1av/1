using System;
using System.Collections.Generic;
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
using Dem5Task_KrasavinSV.windows;

namespace Dem5Task_KrasavinSV.pages
{

    public partial class ProductList : Page
    {

        const int NumInPage = 20;
       
        int PageNum = 0;

        public ProductList()
        {
            InitializeComponent();
            UpdateData();

            List<ProductType> productTypes = EfModel.Init().ProductTypes.ToList();
            productTypes.Insert(0, new ProductType { Title = "Все типы" });

            FilterProducts.ItemsSource = productTypes;
            FilterProducts.SelectedIndex = 0;

            SortProducts.Items.Add("▲ Наименование");
            SortProducts.Items.Add("▼ Наименование");
            SortProducts.Items.Add("▲ Минимальная стоимость");
            SortProducts.Items.Add("▼ Минимальная стоимость");
            SortProducts.Items.Add("▲ Номер цеха");
            SortProducts.Items.Add("▼ Номер цеха");
            
            SortProducts.SelectedIndex = 0;
        }

        private void UpdateData()
        {
            IEnumerable<Product> products = EfModel.Init().Products
                .Where(p => p.Title.Contains(SearchProducts.Text) || p.Description.Contains(SearchProducts.Text));

            if (FilterProducts.SelectedIndex > 0)
                products = products.Where(p => p.ProductType.ID == (FilterProducts.SelectedItem as ProductType).ID);

            switch (SortProducts.SelectedIndex)
            {
                case 0:
                    products = products.OrderBy(p => p.Title);
                    break;
                case 1:
                    products = products.OrderByDescending(p => p.Title);
                    break;
                //...
                case 3:
                    products = products.OrderBy(p => p.ProductionWorkshopNumber);
                    break;
                case 4:
                    products = products.OrderByDescending(p => p.ProductionWorkshopNumber);
                    break;
                case 5:
                    products = products.OrderBy(p => p.MinCostForAgent);
                    break;
                case 6:
                    products = products.OrderByDescending(p => p.MinCostForAgent);
                    break;
            }

            LVProducts.ItemsSource = products.Skip(NumInPage * PageNum).Take(NumInPage).ToList();
            LVProducts.SelectedItems.Clear();

            StackPages.Children.Clear();

            int PageCount = (products.Count() - 1) / NumInPage + 1;

            for (int i = 0; i < PageCount; i++)
            {
                Button button = new Button { Content = new TextBlock { Text = (i + 1).ToString() }, Tag = i };

                button.Click += PageClick;

                if (i == PageNum)

                    (button.Content as TextBlock).TextDecorations = TextDecorations.Underline;

                StackPages.Children.Add(button);
            }

            BackPage.Visibility = Visibility.Visible;
            NextPage.Visibility = Visibility.Visible;

            if (PageNum == 0)
                BackPage.Visibility = Visibility.Collapsed;

            if (PageNum >= PageCount - 1)
                NextPage.Visibility = Visibility.Collapsed;
        }

        private void PageClick(object sender, RoutedEventArgs e)
        {
            PageNum = Convert.ToInt32((sender as Button).Tag);
            UpdateData();
        }

        private void BtChageCostClick(object sender, RoutedEventArgs e)
        {
            IEnumerable<Product> products = LVProducts.SelectedItems.Cast<Product>();
            new ChangeMinCost(products).ShowDialog();
            UpdateData();
        }

        private void BtEditClick(object sender, RoutedEventArgs e)
        {
            if (LVProducts.SelectedItems.Count > 0)
            {
                Product product = LVProducts.SelectedItem as Product;
                NavigationService.Navigate(new ProductEditPage(product));
            }
        }

        private void BtAddClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ProductEditPage(new Product()));        
        }

        private void ProductListVisChanges(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateData();
        }

        private void ClearSelectArea(object sender, RoutedEventArgs e)
        {
            LVProducts.SelectedItems.Clear();
        }

        private void SearchChange(object sender, TextChangedEventArgs e)
        {
            UpdateData();
        }

        private void SortChange(object sender, SelectionChangedEventArgs e)
        {
            UpdateData();
        }

        private void BackPageClick(object sender, RoutedEventArgs e)
        {
            PageNum--;

            UpdateData();
        }

        private void NextPageClick(object sender, RoutedEventArgs e)
        {
            PageNum++;

            UpdateData();
        }
    }
}
