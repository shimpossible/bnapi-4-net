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

using System.ComponentModel;
using System.Net;
using System.IO;

namespace Sample
{
    using BNapi4Net.Diablo3;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        D3Client client;

        public Hero Hero { get; set; }
        public Profile Profile { get; set; }
        public MainWindow()
        {
            client = new D3Client();

            InitializeComponent();            
            this.DataContext = this;

        }

        protected override void OnClosing(CancelEventArgs e)
        {
            client.Dispose();
            base.OnClosing(e);
        }
        public void LoadHero(Hero h)
        {
            this.Hero = h;
            this.OnPropertyChanged("Hero");

            this.Name.Text = h.Name;
            this.Class.Text = h.Level + " " + h.Class;


            string path = "../d3/static/images/profile/hero/paperdoll/" + h.Class + "-" + h.Gender + ".jpg";

            System.IO.Stream s = client.ReadData(path.ToLower(), true);
            System.Windows.Media.Imaging.BitmapImage b = new System.Windows.Media.Imaging.BitmapImage();
            b.CacheOption = BitmapCacheOption.OnLoad;
            b.BeginInit();
            b.StreamSource = s;
            b.EndInit();
            PaperDoll = b;

            OnPropertyChanged("PaperDoll");
        }
        public ImageSource PaperDoll
        {
            get;
            set;
        }
        Item currItem;
        public Item CurrentItem
        {
            get
            {
                return currItem;
            }
            set
            {
                currItem = value;
                OnPropertyChanged("CurrentItem");

                UpdateToolTip(value);
            }
        }

        private void Head_MouseLeave(object sender, MouseEventArgs e)
        {
            toolTip.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void Head_MouseEnter(object sender, MouseEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            Item item = fe.Tag as Item;
            if (item == null) return;

            UpdateToolTip(item);
        }
        private void UpdateToolTip(Item item)
        {
            string path = "../d3/en/tooltip/" + item.TooltipParams;            
            using(Stream st = client.ReadData(path) )
            {
                using (StreamReader rd = new StreamReader(st,  Encoding.UTF8))
                {
                    string html = rd.ReadToEnd();
                    html =
                        "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">" +
                        "<link rel=\"stylesheet\" type=\"text/css\" media=\"all\" href=\"http://us.battle.net/d3/static/css/tooltips.css?v51\" />" +
                        "<link rel=\"stylesheet\" type=\"text/css\" media=\"all\" href=\"http://us.battle.net/d3/static/local-common/css/common-ie.css?v42\" />" +
                        "<meta charset=\"UTF-8\">" +
                        "<body scroll=\"no\" style='padding:0;margin:0;background-color:black' >" +
                        html +
                        "</body>";
                    toolTip.NavigateToString(html);
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string p)
        {
            PropertyChangedEventHandler h = PropertyChanged;
            if (h != null)
            {
                h(this, new PropertyChangedEventArgs(p));
            }
        }
        #endregion

        /// <summary>
        /// Callback when HTML is loaded in WebBrowser control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolTip_LoadCompleted(object sender, NavigationEventArgs e)
        {
            mshtml.HTMLDocument htmlDoc = toolTip.Document as mshtml.HTMLDocument;
            if (htmlDoc != null && htmlDoc.body != null)
            {
                mshtml.IHTMLElement2 body = (mshtml.IHTMLElement2)htmlDoc.body;
                //webBrowser.Width = body.scrollWidth;
                toolTip.Height = body.scrollHeight;
                toolTip.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            string tag = BattleTag.Text;
            Profile = client.GetProfile(tag);
            Profile.Heroes[0].Refresh();
            this.LoadHero(Profile.Heroes[0]);
            this.OnPropertyChanged("Profile");
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Hero h = heroes.SelectedItem as Hero;
            if (h != null)
            {
                h.Refresh();
                this.LoadHero(h);
            }
        }

    }
}
