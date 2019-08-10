using CF.Collections.Generic;
using RestClientDotNet;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AsyncDictionarySample
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private AsyncDictionary<int, GetUsersResponse> _userCache = new AsyncDictionary<int, GetUsersResponse>();

        public MainPage()
        {
            InitializeComponent();
        }

        private async Task<List<User>> GetData(int page)
        {
            try
            {
                TheProgressRing.IsActive = true;

                var isCached = await _userCache.GetContainsKeyAsync(page);
                if (isCached)
                {
                    var cachedResponse = await _userCache.GetValueAsync(page);

                    //Only return a cache hit if the data is not more than 7 seconds old
                    if (cachedResponse.UpdateTime >= (DateTime.Now.AddSeconds(-7)))
                    {
                        return cachedResponse.data;
                    }
                }

                var getUsersResponse = await new RestClient(new NewtonsoftSerializationAdapter(), new Uri("https://reqres.in")).GetAsync<GetUsersResponse>(new Uri($"api/users?page={page}", UriKind.Relative));
                //Make it slow
                await Task.Delay(2000);
                getUsersResponse.UpdateTime = DateTime.Now;
                await _userCache.AddOrReplaceAsync(page, getUsersResponse);

                return getUsersResponse.data;
            }
            catch
            {
                return null;
            }
            finally
            {
                TheProgressRing.IsActive = false;
            }
        }

        private async void GetUsers1_Click(object sender, RoutedEventArgs e)
        {
            UsersBox.ItemsSource = await GetData(1);
        }

        private async void GetUsers2_Click(object sender, RoutedEventArgs e)
        {
            UsersBox.ItemsSource = await GetData(2);
        }

        private async void GetUsers3_Click(object sender, RoutedEventArgs e)
        {
            UsersBox.ItemsSource = await GetData(3);
        }

        private async void GetUsers4_Click(object sender, RoutedEventArgs e)
        {
            UsersBox.ItemsSource = await GetData(4);
        }
    }
}
