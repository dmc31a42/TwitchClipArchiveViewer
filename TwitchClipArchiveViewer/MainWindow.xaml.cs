using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
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
using Path = System.IO.Path;
using Microsoft.WindowsAPICodePack.Dialogs;
using Unosquare.FFME;
using MediaElement = Unosquare.FFME.MediaElement;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using TwitchClipArchiveViewer.Wizard;

namespace TwitchClipArchiveViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<TwitchClip> listTwitchClipToThumbnail = new List<TwitchClip>();
        private int ThumbnailLength;
        private int ThumbnailCount;
        BackgroundWorker worker;

        public MainWindow()
        {
            InitializeComponent();
            //tbFolderPath.Text = @"Z:\트위치\세니카\클립 전체 모음";

            orderSelectors.Add(new OrderSelector
            {
                accentDescent = false, Name = "시간순",
                compareFunc = (twitchClip1, twitchClip2) => DateTime.Compare(twitchClip1.created_at, twitchClip2.created_at)
            });
            orderSelectors.Add(new OrderSelector 
            { 
                accentDescent = true, Name = "조회순" ,
                compareFunc = (twitchClip1, twitchClip2)=> twitchClip1.view_count.CompareTo(twitchClip2.view_count)
            });
            orderSelectors.Add(new OrderSelector
            {
                accentDescent = false,
                Name = "제목순",
                compareFunc = (twitchClip1, twitchClip2) => String.Compare(twitchClip1.title, twitchClip2.title)
            });
            orderSelectors.Add(new OrderSelector 
            { 
                accentDescent = false, Name = "제작자순",
                compareFunc = (twitchClip1, twitchClip2) => String.Compare(twitchClip1.creator_name, twitchClip2.creator_name)
            });
            orderList.ItemsSource = orderSelectors;
            this.DataContext = this;
        }

        public TwitchClip[] twitchClips;
        public TwitchClip[]  twitchClipsFiltered;
        public ObservableCollection<TwitchClip> _twitchClipsFilteredAndOrdered = new ObservableCollection<TwitchClip>();
        public ObservableCollection<TwitchGameStrict> twitchGames;
        public ObservableCollection<OrderSelector> orderSelectors = new ObservableCollection<OrderSelector>();

        public DateTime firstDate;
        public DateTime lastDate;

        public int viewMinimum;
        public int viewMaximum;

        public bool searchEnable = false;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                EnsurePathExists = true,
                EnsureFileExists = false,
                AllowNonFileSystemItems = false,
                IsFolderPicker = true,
                Title = "클립 영상이 모여있는 폴더를 선택하세요"
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                tbFolderPath.Text = dialog.FileName;
            }
        }

        private void tbFolderPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(Directory.Exists(tbFolderPath.Text) == true)
            {
                this.twitchClips = null;
                filterDP.IsEnabled = false;
                pleaseOpenFolder.Visibility = Visibility.Hidden;
                loadingGrid.Visibility = Visibility.Visible;
                string folderPath = tbFolderPath.Text;

                BackgroundWorker worker1 = new BackgroundWorker();
                worker1.DoWork += Worker1_DoWork;
                worker1.RunWorkerCompleted += Worker1_RunWorkerCompleted;
                worker1.RunWorkerAsync(folderPath);
            } else
            {
                MessageBox.Show("올바른 폴더를 선택해주세요.");
            }
        }

        private void Worker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if(e.Result is TwitchClip[] twitchClipsExist)
            {
                filterdpDateFrom.SelectedDate = firstDate;
                filterdpDateFrom.DisplayDateStart = firstDate;
                filterdpDateFrom.DisplayDateEnd = lastDate;
                filterdpDateTo.SelectedDate = lastDate;
                filterdpDateTo.DisplayDateStart = firstDate;
                filterdpDateTo.DisplayDateEnd = lastDate;
                filtertbViewCountFrom.Text = viewMinimum.ToString();
                prevCountFrom = viewMinimum.ToString();
                filtertbViewCountTo.Text = viewMaximum.ToString();
                prevCountTo = viewMaximum.ToString();
                mscbGames.ItemsSource = this.twitchGames;
                if (this.worker != null)
                {
                    this.worker.CancelAsync();
                }
                ThumbnailLength = (e.Result as TwitchClip[]).Count();
                ThumbnailCount = ThumbnailLength - listTwitchClipToThumbnail.Count;
                statusBarThumbnailPB.Value = ThumbnailCount * 100 / ThumbnailLength;
                statusBarThumbnailTB.Text = $"클립 미리보기 생성중 ({ThumbnailCount}/{ThumbnailLength}) ";
                statusBarThumbnailSP.Visibility = Visibility.Visible;
                this.worker = new BackgroundWorker();
                this.worker.WorkerReportsProgress = true;
                this.worker.DoWork += Worker_DoWork;
                this.worker.ProgressChanged += Worker_ProgressChanged;
                this.worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
                this.worker.RunWorkerAsync();

                this.twitchClips = twitchClipsExist;
                filterDP.IsEnabled = true;
                loadingGrid.Visibility = Visibility.Hidden;
                pressSearch.Visibility = Visibility.Visible;
                orderList.IsEnabled = true;

                if(Properties.Settings1.Default.SkipTutorial == false &&
                    Properties.Settings1.Default.Done2 == false)
                {
                    WizardMain wizardMain = new WizardMain(1, 0, false);
                    wizardMain.Show();
                    Properties.Settings1.Default.Done2 = true;
                    Properties.Settings1.Default.Save();
                }
            }
            
        }

        private void Worker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string jsonString = Encoding.UTF8.GetString(Properties.Resource1.senikatwitch_clips_merge);
            TwitchClip[] twitchClips = JsonSerializer.Deserialize<TwitchClip[]>(jsonString);
            string folderPath = e.Argument as string;
            List<TwitchClip> lListTwitchClip = new List<TwitchClip>();
            ParallelOptions parallelOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = -1,
            };
            _ = Parallel.ForEach(twitchClips, parallelOptions, (twitchClip) =>
            //foreach (var twitchClip in twitchClips)
            {
                string filePath = Path.Combine(folderPath, twitchClip.filename);
                if (File.Exists(filePath))
                {
                    twitchClip.download_url = filePath;
                    string thumbnailPath = filePath.Replace(".mp4", ".jpg");
                    if (File.Exists(thumbnailPath) == false)
                    {
                        listTwitchClipToThumbnail.Add(twitchClip);
                        twitchClip.thumbnail_url = "loading.png";
                    }
                    else
                    {
                        FileInfo fileInfo = new FileInfo(filePath);
                        if (fileInfo.Length == 0)
                        {
                            fileInfo.Delete();
                            listTwitchClipToThumbnail.Add(twitchClip);
                            twitchClip.thumbnail_url = "loading.png";
                        }
                        else
                        {
                            twitchClip.thumbnail_url = thumbnailPath;
                        }
                    }
                    lListTwitchClip.Add(twitchClip);
                }
            });
            //}
            if (lListTwitchClip.Count == 0)
            {
                MessageBox.Show("검색된 클립 영상이 없습니다");
            }
            else
            {
                TwitchClip[] twitchClipsExist = lListTwitchClip.ToArray();
                Dictionary<string, TwitchGameStrict> twitchGames = new Dictionary<string, TwitchGameStrict>();
                twitchGames.Add("", new TwitchGameStrict
                {
                    id = "",
                    name = "(이름 없음)",
                    box_art_url = ""
                });
                foreach (var twitchClip in twitchClipsExist)
                {
                    if (twitchClip.game != null)
                    {
                        string id = twitchClip.game.id.ToString();
                        if (id != "0" && twitchGames.ContainsKey(id) == false)
                        {
                            twitchGames.Add(id, new TwitchGameStrict
                            {
                                id = id,
                                name = twitchClip.game.name,
                                box_art_url = twitchClip.game.box_art_url
                            });
                        }
                    }
                }
                this.twitchGames = new ObservableCollection<TwitchGameStrict>(twitchGames.Values.ToArray());
                this.firstDate = twitchClipsExist.Min((twitchClip) => twitchClip.created_at).ToLocalTime();
                this.lastDate = twitchClipsExist.Max((twitchClip) => twitchClip.created_at).ToLocalTime();
                this.viewMinimum = twitchClipsExist.Min((twitchClip) => twitchClip.view_count);
                this.viewMaximum = twitchClipsExist.Max((twitchClip) => twitchClip.view_count);
                e.Result = twitchClipsExist;
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            statusBarThumbnailSP.Visibility = Visibility.Hidden;
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            statusBarThumbnailPB.Value = e.ProgressPercentage;
            statusBarThumbnailTB.Text = $"클립 미리보기 생성중 ({ThumbnailCount}/{ThumbnailLength}) ";
        }

        //https://www.csharpstudy.com/WinForms/WinForms-backgroundworker.aspx
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (sender is BackgroundWorker backgroundWorker)
            {
                ParallelOptions parallelOptions = new ParallelOptions()
                {
                    MaxDegreeOfParallelism = 10
                };
                Parallel.ForEach(listTwitchClipToThumbnail, parallelOptions, (twitchClip, action, i) =>
                //foreach (var twitchClip in listTwitchClipToThumbnail)
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(@"ffmpeg\ffmpeg.exe");
                    startInfo.Arguments = "-y -itsoffset -4  -i \"" + twitchClip.download_url + "\" -vcodec mjpeg -vframes 1 -an -f rawvideo -s 320x180 \"" + twitchClip.download_url.Replace(".mp4", ".jpg") + "\"";
                    startInfo.CreateNoWindow = true;
                    startInfo.UseShellExecute = false;
                    startInfo.RedirectStandardOutput = true;
                    startInfo.RedirectStandardError = true;
                    startInfo.RedirectStandardInput = true;
                    bool Success = true;
                    do
                    {
                        Success = true;
                        Process proc = Process.Start(startInfo);
                        DateTime dateTime = DateTime.Now;
                        System.Threading.Timer timer = new Timer((state) =>
                        {
                            proc.Kill();
                            Success = false;
                        },null, 10 * 1000, System.Threading.Timeout.Infinite);
                        do
                        {
                            string output = proc.StandardOutput.ReadToEnd();
                            string error = proc.StandardError.ReadToEnd();
                            //Log.StreamWriter.WriteLine(output);
                            //Log.StreamWriter.WriteLine(error);
                            //if(error.IndexOf("already exists. Overwrite? [y/N]") != -1)
                            //{
                            //    proc.StandardInput.WriteLine("y");
                            //}
                        } while (!proc.HasExited && (DateTime.Now-dateTime).TotalSeconds<10);
                        if(Success != false && proc.HasExited)
                        {
                        } else
                        {
                            Success = false;
                            proc.Kill();
                            //Log.StreamWriter.WriteLine("****************ERROR****************");
                            //Log.StreamWriter.WriteLine("Timeout");
                            //Log.StreamWriter.WriteLine(twitchClip.download_url);
                            if (File.Exists(twitchClip.download_url.Replace(".mp4", ".jpg")))
                            {
                                //Log.StreamWriter.WriteLine("Delete: " + twitchClip.download_url.Replace(".mp4", ".jpg"));
                                File.Delete(twitchClip.download_url.Replace(".mp4", ".jpg"));
                            }
                        }
                        timer.Dispose();
                    } while (Success == false);
                    Interlocked.Increment(ref this.ThumbnailCount);
                    twitchClip.thumbnail_url = twitchClip.download_url.Replace(".mp4", ".jpg");
                    backgroundWorker.ReportProgress(this.ThumbnailCount * 100 / this.ThumbnailLength);
                    if (e.Cancel)
                    {
                        action.Break();
                        //break;
                    }
                //}
                });
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(sender is Grid  grid)
            {
                ClipPlayer clipPlayer = new ClipPlayer(grid.DataContext as TwitchClip);

                clipPlayer.ShowDialog();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(twitchClips != null)
            {
                filterDP.IsEnabled = false;
                string title = filtertbTitle.Text;
                DateTime dateFrom = filterdpDateFrom.SelectedDate ?? this.firstDate;
                DateTime dateTo = filterdpDateTo.SelectedDate ?? this.lastDate;
                if(dateFrom>dateTo)
                {
                    DateTime temp = dateTo;
                    dateTo = dateFrom;
                    dateFrom = temp;
                    filterdpDateFrom.SelectedDate = dateFrom;
                    filterdpDateTo.SelectedDate = dateTo;
                }
                dateTo = dateTo.AddDays(1);
                string creator = filtertbCreator.Text;
                int viewCountFrom = viewMinimum;
                int.TryParse(filtertbViewCountFrom.Text.Replace(",", ""), out viewCountFrom);
                int viewCountTo = viewMaximum;
                int.TryParse(filtertbViewCountTo.Text.Replace(",", ""), out viewCountTo);
                if(viewCountFrom>viewCountTo)
                {
                    int temp = viewCountFrom;
                    viewCountFrom = viewCountTo;
                    viewCountTo = temp;
                    filtertbViewCountFrom.Text = viewCountFrom.ToString();
                    filtertbViewCountTo.Text = viewCountTo.ToString();
                }
                pressSearch.Visibility = Visibility.Hidden;
                duringSearch.Visibility = Visibility.Visible;
                BackgroundWorker workerSearch = new BackgroundWorker();
                workerSearch.DoWork += (sender, e) =>
                {
                    Dictionary<string, TwitchGameStrict> selectedGames = new Dictionary<string, TwitchGameStrict>();
                    foreach (TwitchGameStrict twitchGame in twitchGameSelected)
                    {
                        selectedGames.Add(twitchGame.id, twitchGame);
                    }
                    twitchClipsFiltered = twitchClips.Where((twitchClip) =>
                    {
                        if (String.IsNullOrEmpty(title) == false)
                        {
                            if (twitchClip.title.Contains(title, StringComparison.CurrentCultureIgnoreCase) == false)
                            {
                                return false;
                            }
                        }
                        if (String.IsNullOrEmpty(creator) == false)
                        {
                            if (twitchClip.creator_name.Contains(creator, StringComparison.CurrentCultureIgnoreCase) == false)
                            {
                                return false;
                            }
                        }
                        if (viewCountFrom <= twitchClip.view_count && twitchClip.view_count <= viewCountTo)
                        {

                        }
                        else
                        {
                            return false;
                        }
                        if (dateFrom <= twitchClip.created_at.ToLocalTime() && twitchClip.created_at.ToLocalTime() <= dateTo)
                        {

                        }
                        else
                        {
                            return false;
                        }
                        if (selectedGames.Count != 0)
                        {
                            if (selectedGames.ContainsKey(twitchClip.game_id) == false)
                            {
                                return false;
                            }
                        }
                        return true;
                    }).ToArray();
                    Sorting();
                };
                workerSearch.RunWorkerCompleted += (sender, e) =>
                {
                    if (this.twitchClipsFiltered != null)
                    {
                        statusBarSearchResult.Text = "검색된 클립: " + twitchClipsFiltered.Count().ToString() + "개";
                        duringSearch.Visibility = Visibility.Hidden;
                        if (Properties.Settings1.Default.SkipTutorial == false)
                        {
                            if (Properties.Settings1.Default.Done3 == false)
                            {
                                WizardMain wizardMain = new WizardMain(2, 0, false);
                                wizardMain.Show();
                                Properties.Settings1.Default.Done3 = true;
                                Properties.Settings1.Default.Save();
                            }
                            else if (Properties.Settings1.Default.Done4 == false)
                            {
                                WizardMain wizardMain = new WizardMain(3, 0, false);
                                wizardMain.Show();
                                Properties.Settings1.Default.Done4 = true;
                                Properties.Settings1.Default.Save();
                            }
                        }
                        lv1.ItemsSource = new ObservableCollection<TwitchClip>(this.twitchClipsFiltered);
                        filterDP.IsEnabled = true;
                    }
                };
                workerSearch.RunWorkerAsync();
                
            } else
            {
               
            }
        }

        private void Sorting()
        {
            if(this.twitchClipsFiltered != null)
            {
                
                Array.Sort(this.twitchClipsFiltered, (twitchClip1, twitchClip2) =>
                {
                    foreach (var orderSelector in orderSelectors)
                    {
                        int result = orderSelector.compareTo(twitchClip1, twitchClip2);
                        if (result == 0)
                        {
                            continue;
                        }
                        else
                        {
                            return result;
                        }
                    }
                    return 0;
                });
            }
            
        }

        string prevCountFrom;
        private void filtertbViewCountFrom_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(sender is TextBox textBox)
            {
                int i;
                if(int.TryParse(textBox.Text.Replace(",",""), out i)) {
                    prevCountFrom = textBox.Text;
                } else
                {
                    textBox.Text = prevCountFrom;
                    textBox.Select(textBox.Text.Length, 0);
                }
            }
        }

        string prevCountTo;
        private void filtertbViewCountTo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                int i;
                if (int.TryParse(textBox.Text.Replace(",", ""), out i)) {
                    prevCountTo = textBox.Text;
                }
                else
                {
                    textBox.Text = prevCountTo;
                    textBox.Select(textBox.Text.Length, 0);
                }
            }
        }

        private void filtertbTitle_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Return)
            {
                Button_Click_1(sender, null);
            }
        }

        private void filterdpDateFrom_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            Button_Click_1(sender, null);
        }

        IEnumerable<TwitchGameStrict> twitchGameSelected = new Collection<TwitchGameStrict>();
        private void mscbGames_SelectedItemsChanged(object sender, Sdl.MultiSelectComboBox.EventArgs.SelectedItemsChangedEventArgs e)
        {
            twitchGameSelected = e.Selected.Cast<TwitchGameStrict>();
        }

        private void orderList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(sender is ContentPresenter draggedItem)
            {
                DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
            }
        }

        void SortingAsync()
        {
            BackgroundWorker sortWorker = new BackgroundWorker();
            sortWorker.DoWork += (sender, e) =>
            {
                Sorting();
            };
            sortWorker.RunWorkerCompleted += (sender, e) =>
            {
                lv1.ItemsSource = new ObservableCollection<TwitchClip>(this.twitchClipsFiltered);
            };
            sortWorker.RunWorkerAsync();
        }

        private void orderList_Drop(object sender, DragEventArgs e)
        {
            OrderSelector droppedData = e.Data.GetData(typeof(OrderSelector)) as OrderSelector;
            OrderSelector target = ((ContentPresenter)(sender)).DataContext as OrderSelector;

            int removedIdx = orderList.Items.IndexOf(droppedData);
            int targetIdx = orderList.Items.IndexOf(target);

            if(removedIdx < targetIdx)
            {
                orderSelectors.Insert(targetIdx + 1, droppedData);
                orderSelectors.RemoveAt(removedIdx);
                SortingAsync();
            } else
            {
                int remIdx = removedIdx + 1;
                if(orderSelectors.Count+1 > remIdx)
                {
                    orderSelectors.Insert(targetIdx, droppedData);
                    orderSelectors.RemoveAt(remIdx);
                    SortingAsync();
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if(sender is Button button)
            {
                if(button.DataContext is OrderSelector orderSelector)
                {
                    orderSelector.accentDescent = !orderSelector.accentDescent;
                    SortingAsync();
                }
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                // Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
                Properties.Settings1.Default.Top = RestoreBounds.Top;
                Properties.Settings1.Default.Left = RestoreBounds.Left;
                Properties.Settings1.Default.Height = RestoreBounds.Height;
                Properties.Settings1.Default.Width = RestoreBounds.Width;
                Properties.Settings1.Default.Maximized = true;
            }
            else
            {
                Properties.Settings1.Default.Top = this.Top;
                Properties.Settings1.Default.Left = this.Left;
                Properties.Settings1.Default.Height = this.Height;
                Properties.Settings1.Default.Width = this.Width;
                Properties.Settings1.Default.Maximized = false;
            }

            Properties.Settings1.Default.Save();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            this.Top = Properties.Settings1.Default.Top;
            this.Left = Properties.Settings1.Default.Left;
            this.Height = Properties.Settings1.Default.Height;
            this.Width = Properties.Settings1.Default.Width;
            // Very quick and dirty - but it does the job
            if (Properties.Settings1.Default.Maximized)
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if(sender is Button button)
            {
                TwitchClip twitchClip = button.DataContext as TwitchClip;
                ProcessStartInfo startInfo = new ProcessStartInfo(@"explorer");
                startInfo.Arguments = $"/select,\"{twitchClip.download_url}\"";
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                //startInfo.RedirectStandardOutput = true;
                //startInfo.RedirectStandardError = true;
                Process proc = Process.Start(startInfo);
            }
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if(Properties.Settings1.Default.SkipTutorial == false &&
                Properties.Settings1.Default.Done1 == false)
            {
                WizardMain wizardMain = new WizardMain(0, 0, false);
                wizardMain.Show();
                Properties.Settings1.Default.Done1 = true;
                Properties.Settings1.Default.Save();
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            WizardMain wizardMain = new WizardMain(0, 0, true);
            wizardMain.Show();
        }
    }
}
