using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dem5Task_KrasavinSV.db
{
    // Creating class for read and import csv file to the database
    static class ImportClass
    {
        public static void ImportMaterial()
        {
            string[] lines = File.ReadAllLines(@"..\..\..\csv\materials_short_b_import (csv).csv"); // C:\Users\stask\Source\Repos\

            Random random = new Random();

            foreach (string line in lines.Skip(1))
            {
                string[] items = line.Split(',').Select(I => I.Trim()).ToArray();

                // создаем экземпляр класса подключения к бд, который обращается к сущности MaterialType и ищет первое или
                // дефолтное значение title и присваивает его переменной mt.
                MaterialType materialType = EfModel.Init().MaterialTypes.ToArray().FirstOrDefault(mt => mt.Title == items[1]);

                // Проверяем materialType на нулевое значение
                if (materialType == null)
                {
                    materialType = new MaterialType
                    {
                        Title = items[1],
                        DefectedPercent = random.NextDouble() + random.Next(0, 3)
                    };
                    // Инициируем подключение к сущности MT и обновляем данные в ней
                    EfModel.Init().MaterialTypes.Add(materialType);
                }
                
                Material material = new Material
                {
                    Title = items[0],
                    MaterialType = materialType,
                    CountInPack = Convert.ToInt32(items[2]),
                    Unit = items[3],
                    CountInStock = Convert.ToInt32(items[4]),
                    MinCount = Convert.ToDouble(items[5]),
                    Cost = Convert.ToDecimal(items[6])
                };
                // Инициируем подключение к сущности Material и обновляем данные в нём
                EfModel.Init().Materials.Add(material);
                // Saving changes to database
                EfModel.Init().SaveChanges();
            }
        }

        public static void ProductImport()
        {
            string[] lines = File.ReadAllLines(@"..\..\..\csv\products_b_import (csv).csv");

            Random random = new Random();

            foreach (string line in lines.Skip(1))
            {

                string[] items = line.Split(',').Select(I => I.Trim()).ToArray();

                ProductType productType = EfModel.Init().ProductTypes.ToArray().FirstOrDefault(pt => pt.Title == items[4]);
                
                if (productType == null)
                {
                    productType = new ProductType
                    {
                        Title = items[4],
                        DefectedPercent = random.NextDouble() + random.Next(0, 3)
                    };

                    EfModel.Init().ProductTypes.Add(productType);
                }

                Product product = new Product
                {
                    Title = items[0],
                    ArticleNumber = items[1],
                    MinCostForAgent = Convert.ToDecimal(items[2]),
                    ProductType = productType,
                    ProductionPersonCount = Convert.ToInt32(items[5]),
                    ProductionWorkshopNumber = Convert.ToInt32(items[6])              
                };

                try
                {
                    product.Image = File.ReadAllBytes(@"..\..\resources\furniture\" + items[3]);
                }
                catch
                {
                    Console.WriteLine(items[3]);
                }
                EfModel.Init().Products.Add(product);
                EfModel.Init().SaveChanges();
            }
        }

        public static void ImportProductMaterial()
        {
            string[] lines = File.ReadAllLines(@"..\..\..\csv\productmaterial_b_import (csv).csv");

            foreach (string line in lines.Skip(1))
            {
                string[] items = line.Split(',').Select(I => I.Trim()).ToArray();

                Product product = EfModel.Init().Products.ToArray().First(p => p.Title == items[0]);

                Material material = EfModel.Init().Materials.ToArray().First(m => m.Title == items[1]);

                ProductMaterial productMaterial = new ProductMaterial
                {
                    Product = product,
                    Material = material,
                    Count = Convert.ToDouble(items[2])
                };

                EfModel.Init().ProductMaterials.Add(productMaterial);
            }
            EfModel.Init().SaveChanges();
        }
    }
}
