using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.App.Views;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System.Collections.ObjectModel;
using Grocery.Core.Data.Repositories;

namespace Grocery.App.ViewModels
{
    [QueryProperty(nameof(GroceryList), nameof(GroceryList))]
    public partial class GroceryListItemsViewModel : BaseViewModel
    {
        private readonly IGroceryListItemsService _groceryListItemsService;
        private readonly IProductService _productService;
        public ObservableCollection<GroceryListItem> MyGroceryListItems { get; set; } = [];
        public ObservableCollection<Product> AvailableProducts { get; set; } = [];

        [ObservableProperty]
        GroceryList groceryList = new(0, "None", DateOnly.MinValue, "", 0);

        public GroceryListItemsViewModel(IGroceryListItemsService groceryListItemsService, IProductService productService)
        {
            _groceryListItemsService = groceryListItemsService;
            _productService = productService;
            Load(groceryList.Id);
        }

        private void Load(int id)
        {
            MyGroceryListItems.Clear();
            foreach (var item in _groceryListItemsService.GetAllOnGroceryListId(id)) MyGroceryListItems.Add(item);
            GetAvailableProducts();
        }

        private void GetAvailableProducts()
        {
            AvailableProducts.Clear();
            
            var productRepository = new ProductRepository();
            
            foreach (var product in productRepository.GetAll())
            {
                if (product.Stock <= 0 ) continue;
                var alreadyInList = MyGroceryListItems.Any(item => item.ProductId == product.Id);

                if (!alreadyInList)
                {
                    AvailableProducts.Add(product);
                }
            }
        }

        partial void OnGroceryListChanged(GroceryList value)
        {
            Load(value.Id);
        }

        [RelayCommand]
        public async Task ChangeColor()
        {
            Dictionary<string, object> paramater = new() { { nameof(GroceryList), GroceryList } };
            await Shell.Current.GoToAsync($"{nameof(ChangeColorView)}?Name={GroceryList.Name}", true, paramater);
        }
        [RelayCommand]
        public void AddProduct(Product product)
        {
            if (product == null || product.Id == 0) return;
            if (GroceryList == null || GroceryList.Id == 0) return;
            
            var groceryListItem = new GroceryListItem(
                0,
                GroceryList.Id,
                product.Id,
                1
            );
            _groceryListItemsService.Add(groceryListItem);

            product.Stock -= 1;
            _productService.Update(product);

            if (product.Stock <= 0)
            {
                var toRemove = AvailableProducts.FirstOrDefault(p => p.Id == product.Id);
                if (toRemove != null)
                {
                    AvailableProducts.Remove(toRemove);
                }
            }
            OnGroceryListChanged(GroceryList);
        }
    }
}
