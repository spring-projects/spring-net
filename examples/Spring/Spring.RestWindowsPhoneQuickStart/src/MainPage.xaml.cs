using System;
using System.Windows;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Phone.Controls;

using Spring.Http.Client;
using Spring.Http.Rest;

namespace Spring.RestWindowsPhoneQuickStart
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void GetButton_Click(object sender, RoutedEventArgs e)
        {
            RestTemplate rt = new RestTemplate("http://twitter.com");

            rt.GetForObjectAsync<TwitterStatuses>("/statuses/user_timeline.xml?screen_name={name}", 
                args =>
                {
                    if (args.Error == null)
                    {
                        this.StatusesListBox.ItemsSource = args.Response;
                    }
                }, this.TwitterAccountTextBox.Text);

            //rt.GetForObjectAsync<XElement>("/statuses/user_timeline.xml?screen_name={name}",  
            //    args =>
            //    {
            //        if (args.Error == null)
            //        {
            //            this.StatusesListBox.ItemsSource = from tweet in args.Response.Descendants("status")
            //                                               select new TwitterItem
            //                                               {
            //                                                   ImageSource = tweet.Element("user").Element("profile_image_url").Value,
            //                                                   Message = tweet.Element("text").Value,
            //                                                   UserName = tweet.Element("user").Element("screen_name").Value
            //                                               };
            //        }
            //    }, this.TwitterAccountTextBox.Text);
        }
    }

    [CollectionDataContract(Name="statuses", ItemName="status", Namespace="")]
    public class TwitterStatuses : List<TwitterStatus>
    {
    }

    [DataContract(Name = "status", Namespace = "")]
    public class TwitterStatus
    {
        [DataMember(Name="text")]
        public string Text { get; set; }

        [DataMember(Name = "user")]
        public TwitterUser User { get; set; }
    }

    [DataContract(Name="user", Namespace="")]
    public class TwitterUser
    {
        [DataMember(Name = "screen_name")]
        public string ScreenName { get; set; }

        [DataMember(Name = "profile_image_url")]
        public string ImageUrl { get; set; }
    }

    public class TwitterItem
    {
        public string UserName { get; set; }
        public string Message { get; set; }
        public string ImageSource { get; set; }
    }
}