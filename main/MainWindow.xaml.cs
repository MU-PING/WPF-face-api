
using System.Text;
using System.Windows;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Web;

namespace Azure_face_recognition
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        // 填入自己Azure Face API's key
        string apiKey = "13e80e2bb7e0491090e65a6b25967156";
        string persongroupid = "";
        string consoletext = "";
        ObservableCollection<Person> personData = new ObservableCollection<Person>();
        Person person;
        
        public MainWindow()
        {
            InitializeComponent();
            consoletext = ConsoleTextBlock.Text;
        }

        async void CreatPersonGroup(object sender, RoutedEventArgs e) // Put API
        {

            HttpResponseMessage response;
            string result;

            // ID 跟 Name 都可以設定，但 ID 最多只能建立一次，Name 則沒有此限制，如同不同身分字號但可以叫相同名字
            persongroupid = PersonGroupID.Text;
            string persongroupname = PersonGroupName.Text;
            
            // Creat HttpClient
            var client = new HttpClient();

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
            var uri = "https://centralus.api.cognitive.microsoft.com/face/v1.0/persongroups/" + persongroupid;

            // Request body, all parametersa: name、userData(optional)、recognitionModel(optional)
            byte[] byteData = Encoding.UTF8.GetBytes("{\"name\": \"" + persongroupname + "\"}");

            using (var content = new ByteArrayContent(byteData))
            {
                // 設定Content-Type：可以設定也可以不設定，基本上就是application/json 一般來說Content-Type可以直接加在header的Json中，但 HttpClient 必須將其分開
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PutAsync(uri, content);
                result = await response.Content.ReadAsStringAsync();
            }

            // 更新ConsoleTex
            if (string.IsNullOrEmpty(result))
            {
                consoletext = consoletext + "\n" + "PersonGroup successfully create";
            }
            else
            {
                consoletext = consoletext + "\n" + result;
            }
            ConsoleTextBlock.Text = consoletext;

            // 關閉TextBox PersonGroupName
            PersonGroupID.IsEnabled = false;
            PersonGroupName.IsEnabled = false;
        }

        async void CreatPerson(object sender, RoutedEventArgs e)  // Post API
        {
            HttpResponseMessage response;
            string result;
            string personname = PersonName.Text;

            var jstr = new
            {
                name = personname
            };

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
            var uri = "https://centralus.api.cognitive.microsoft.com/face/v1.0/persongroups/" + persongroupid + "/persons";
            byte[] byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jstr));

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(uri, content);
                result = await response.Content.ReadAsStringAsync();
            }

            // 更新ConsoleTex
            consoletext = consoletext + "\n" + result;
            ConsoleTextBlock.Text = consoletext;

            // 解析API回傳的JSON
            JObject resultJSON = JObject.Parse(result);

            // 將創建的人展現在DataGrid
            person = new Person
            {
                name = personname,
                id = resultJSON["personId"].ToString(),
                face = 0
            };

            // 更新DataGrid
            personData.Add(person);
            dataGrid.ItemsSource = personData;
        }

        async void AddFace(object sender, RoutedEventArgs e)  // Post API
        {
            HttpResponseMessage response;
            string result;
            string personfaceurl = PersonFaceURL.Text;

            var jstr = new
            {
                url = personfaceurl
            };

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
            var uri = "https://centralus.api.cognitive.microsoft.com/face/v1.0/persongroups/" + persongroupid + "/persons/" + person.id + "/persistedFaces";
            byte[] byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jstr));

            using (var content = new ByteArrayContent(byteData))
            {
                //1. For an image URL, Content-Type should be "application/json"
                //2. For a local image, Content-Type should be "application / octet - stream"
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(uri, content);
                result = await response.Content.ReadAsStringAsync();
            }

            // 更新ConsoleTex
            consoletext = consoletext + "\n" + result;
            ConsoleTextBlock.Text = consoletext;

            // 更新DataGrid，先將舊資料移除 不然不能更新
            personData.Last().face += 1;
            dataGrid.ItemsSource = null;
            dataGrid.ItemsSource = personData;

        }
        async void Train(object sender, RoutedEventArgs e)  // Post API
        {

            HttpResponseMessage response;
            string result;

            // Request headers
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
            var uri = "https://centralus.api.cognitive.microsoft.com/face/v1.0/personGroups/" + persongroupid + "/train";

            // Request body
            byte[] byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject("{}"));

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(uri, content);
                result = await response.Content.ReadAsStringAsync();
            }

            // 更新ConsoleTex
            if (string.IsNullOrEmpty(result))
            {
                consoletext = consoletext + "\n" + "Model successfully train";
            }
            else
            {
                consoletext = consoletext + "\n" + result;
            }
            ConsoleTextBlock.Text = consoletext;

        }

        async void Test(object sender, RoutedEventArgs e)  
        {

            HttpResponseMessage response;
            string result;
            var client = new HttpClient();
            var uri="";
            byte[] byteData;
            JObject resultJSON;

            // Detect (Post API)------------------------------ 取得FaceID
            var Detect_jstr = new
            {
                url = PreviewURL.Text
            };

            var queryString = HttpUtility.ParseQueryString("returnFaceId=true");
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);

            uri = "https://centralus.api.cognitive.microsoft.com/face/v1.0/detect/?" + queryString;
            byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Detect_jstr));

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(uri, content);
                result = await response.Content.ReadAsStringAsync();
            }
            resultJSON = JObject.Parse(result.Substring(1, result.Length - 2));


            // Identify (Post API)------------------------------ 使用 FaceID 去對應的 PersonGroup 辨識，取回 PersonID
            var Identify_jstr = new
            {
                personGroupId = persongroupid,
                faceIds = new string[] { resultJSON["faceId"].ToString() },
                maxNumOfCandidatesReturned = 1,
                confidenceThreshold = 0.5,
            };

            uri = "https://centralus.api.cognitive.microsoft.com/face/v1.0/identify";
            byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Identify_jstr));

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                //Console.WriteLine(uri);
                response = await client.PostAsync(uri, content);
                result = await response.Content.ReadAsStringAsync();
            }
            resultJSON = JObject.Parse(result.Substring(1, result.Length - 2));
            consoletext = consoletext + "\n" + "Confidence: " + resultJSON["candidates"].ToArray()[0]["confidence"].ToString();

            // Get (Get API)------------------------------ 使用 PersonID 去 PersonGroup 取回 Name

            uri = "https://centralus.api.cognitive.microsoft.com/face/v1.0/personGroups/" + persongroupid + "/persons/" + resultJSON["candidates"].ToArray()[0]["personId"].ToString();

            // client.GetAsync 不用放 Content-Type
            response = await client.GetAsync(uri);
            result = await response.Content.ReadAsStringAsync();

            resultJSON = JObject.Parse(result);
            consoletext = consoletext + "\n" + "Name: " + resultJSON["name"].ToString();

            // 更新ConsoleTex
            ConsoleTextBlock.Text = consoletext;
        }

        void Preview(object sender, RoutedEventArgs e)  // Post API
        {
            string url = PreviewURL.Text;

            // Create source
            BitmapImage myBitmapImage = new BitmapImage();

            // BitmapImage.UriSource must be in a BeginInit/EndInit block
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(url);

            // To save significant application memory, set the DecodePixelWidth or
            // DecodePixelHeight of the BitmapImage value of the image source to the desired
            // height or width of the rendered image. If you don't do this, the application will
            // cache the image as though it were rendered as its normal size rather then just
            // the size that is displayed.
            // Note: In order to preserve aspect ratio, set DecodePixelWidth
            // or DecodePixelHeight but not both.

            myBitmapImage.DecodePixelWidth = 800;
            myBitmapImage.EndInit();
            //set image source
            PreviewImg.Source = myBitmapImage;
        }
    }
    public class Person
    {
        public string name { get; set; }
        public string id { get; set; }
        public int face { get; set; }
    }
}
