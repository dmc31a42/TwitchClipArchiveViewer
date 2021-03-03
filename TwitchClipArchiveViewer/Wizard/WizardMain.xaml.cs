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

namespace TwitchClipArchiveViewer.Wizard
{
    /// <summary>
    /// WizardMain.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WizardMain : Window
    {
        private int index1 = 0;
        private int index2 = 0;
        private bool allPage = false;
        private List<List<Page>> pages = new List<List<Page>>();
        public WizardMain(int group, int page, bool allPage)
        {
            index1 = group;
            index2 = page;
            this.allPage = allPage;
            InitializeComponent();

            List<Page> wizard1 = new List<Page>();
            wizard1.Add(new Wizard1.Welcome());
            wizard1.Add(new Wizard1.FolderOpen());
            pages.Add(wizard1);

            List<Page> wizard2 = new List<Page>();
            wizard2.Add(new Wizard2.BackgroundThumbnailWork());
            wizard2.Add(new Wizard2.AnywaySearch());
            pages.Add(wizard2);

            List<Page> wizard3 = new List<Page>();
            wizard3.Add(new Wizard3.TitleSearch());
            wizard3.Add(new Wizard3.DateSearch());
            wizard3.Add(new Wizard3.CreatorSearch());
            wizard3.Add(new Wizard3.ViewCountSearch());
            wizard3.Add(new Wizard3.GameSearch());
            wizard3.Add(new Wizard3.SearchEnd());
            pages.Add(wizard3);

            List<Page> wizard4 = new List<Page>();
            wizard4.Add(new Wizard4.OrderSelector());
            wizard4.Add(new Wizard4.ClipItem());
            wizard4.Add(new Wizard4.FinishTutorial());
            pages.Add(wizard4);

            this.frame.Navigate(pages[index1][index2]);
        }

        public bool AllPage
        {
            get => allPage;
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            this.frame.NavigationService.GoBack();
            if(this.frame.NavigationService.CanGoBack == false)
            {
                this.PrevButton.IsEnabled = false;
            }
            index2--;
            if(index2 == -1)
            {
                index1--;
                index2 = pages[index1].Count - 1;
            }
            if (pages[index1].Count - 1 == index2)
            {
                if (allPage == false)
                {
                    NextButton.IsEnabled = false;
                }
                else
                {
                    if (pages.Count - 1 == index1)
                    {
                        NextButton.IsEnabled = false;
                    } else
                    {
                        NextButton.IsEnabled = true;
                    }
                }
            }
            else if (pages[index1].Count == index2)
            {
                if (allPage == false)
                {
                    throw new Exception();
                }
                else
                {
                    //index1++;
                    //index2 = 0;
                }
            }
            else
            {
                NextButton.IsEnabled = true;
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            index2++;
            if(pages[index1].Count-1 == index2)
            {
                if(allPage == false)
                {
                    NextButton.IsEnabled = false;
                } else
                {
                    if(pages.Count-1 == index1)
                    {
                        NextButton.IsEnabled = false;
                    }
                    else
                    {
                        NextButton.IsEnabled = true;
                    }
                }
            } else if(pages[index1].Count == index2)
            {
                if(allPage == false)
                {
                    throw new Exception();
                } else
                {
                    index1++;
                    index2 = 0;
                }
            } else
            {
                NextButton.IsEnabled = true;
            }
            PrevButton.IsEnabled = true;
            this.frame.NavigationService.Navigate(pages[index1][index2]);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings1.Default.SkipTutorial = true;
            Properties.Settings1.Default.Save();
            MessageBox.Show("추후 튜토리얼을 다시 보고 싶다면 앱 화면 왼쪽 하단의 ? 버튼을 클릭하세요");
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings1.Default.SkipTutorial = false;
            Properties.Settings1.Default.Save();
        }
    }
}
