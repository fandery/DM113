﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using ProductsEntityModel;
using System.ServiceModel.Activation;

namespace Products
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class ProductsService : IProductsService, IProductsServiceV2
    {
        public bool AddStock(string productCode, decimal quantity)
        {
            try
            {
                // Connect to the ProductsModel database                  
                using (ProductsModel database = new ProductsModel())
                {
                    if (ProductExists(productCode, database))
                    {
                        // Find the ProductID for the specified product                     
                        int productID = (from p in database.Products where String.Compare(p.Code, productCode) == 0 select p.Id).First();

                        // Find the Stock object that matches the parameters passed                     
                        // in to the operation                     
                        Stock stock = database.Stocks.First(pi => pi.Id == productID);
                        stock.Quantity = quantity;
                        stock.LastUpdate = DateTime.Now;

                        database.Stocks.Add(stock);

                        database.SaveChanges();
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public int CurrentStock(string productCode)
        {
            int quantityTotal = 0;
            try
            {
                // Connect to the ProductsModel database                  
                using (ProductsModel database = new ProductsModel())
                {

                    if (ProductExists(productCode, database))
                    {
                        // Calculate the sum of all quantities for the specified product
                        quantityTotal = (from s in database.Stocks
                                         join p in database.Products on s.Product.Id equals p.Id
                                         where String.Compare(p.Code, productCode) == 0
                                         select (int)s.Quantity).Sum();
                    }
                }
            }
            catch
            {
                // Ignore exceptions in this implementation              
            }
            // Return the stock level              
            return quantityTotal;
        }

        public ProductData GetProduct(string productCode)
        {
            ProductData productData = null;
            try
            {
                using (ProductsModel database = new ProductsModel())
                {
                    if (ProductExists(productCode, database)){
                        Product matchingProduct = database.Products.First(p => String.Compare(p.Code, productCode) == 0);
                        productData = new ProductData()
                        {
                            Name = matchingProduct.Name,
                            Code = matchingProduct.Code,
                            Price = matchingProduct.Price
                        };
                    }
                }
            }
            catch
            {
                // Ignore exceptions in this implementation              
            }

            return productData;
        }

        public List<ProductData> ListMatchingProducts(string match)
        {
            // Create a list of products            
            List<ProductData> productsList = new List<ProductData>();
            try
            {
                using (ProductsModel database = new ProductsModel())
                {
                    List<Product> products = (from product in database.Products
                                              where product.Name.Contains(match)
                                              select product).ToList();

                    foreach (Product product in products)
                    {
                        ProductData productData = new ProductData()
                        {
                            Name = product.Name,
                            Code = product.Code,
                            Price = product.Price
                        };
                        productsList.Add(productData);
                    }
                }
            }
            catch
            {
                // Ignore exceptions in this implementation   
            }
            return productsList;
        }

        public List<ProductData> ListProducts()
        {
            return ListMatchingProducts("");
        }

        public bool ProductExists(string productCode, ProductsModel database)
        {
            // Check to see whether the specified product exists in the database
            int numProducts = (from p in database.Products
                               where string.Equals(p.Code, productCode)
                               select p).Count();
            return numProducts > 0;
        }
      }
    }
