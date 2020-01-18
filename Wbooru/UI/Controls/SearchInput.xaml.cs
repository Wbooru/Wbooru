using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Wbooru.Models;
using Wbooru.Persistence;
using Wbooru.Galleries;
using System.Threading;
using System.Windows.Threading;
using Wbooru.Galleries.SupportFeatures;
using Wbooru.Settings;

namespace Wbooru.UI.Controls
{
    using Logger = Log<SearchInput>;

    /// <summary>
    /// SearchInput.xaml 的交互逻辑
    /// </summary>
    public partial class SearchInput : UserControl
    {
        Storyboard ShowInputAction, HideInputAction;
        DoubleAnimation animation;

        public ObservableCollection<Tag> Suggests { get; set; } = new ObservableCollection<Tag>();
        LocalDBContext localDB;

        public Gallery SearchTagGallery
        {
            get { return (Gallery)GetValue(SearchTagGalleryProperty); }
            set { SetValue(SearchTagGalleryProperty, value); }
        }

        public static readonly DependencyProperty SearchTagGalleryProperty =
            DependencyProperty.Register("SearchTagGallery", typeof(Gallery), typeof(SearchInput), new PropertyMetadata(default));

        public SearchInput()
        {
            InitializeComponent();

            MainPanel.DataContext = this;

            ShowInputAction = Resources["ShowInputAction"] as Storyboard;
            HideInputAction = Resources["HideInputAction"] as Storyboard;
            animation = ShowInputAction.Children.OfType<DoubleAnimation>().First();

            localDB = LocalDBContext.Instance;
        }

        public bool _mouse_enter = false;
        public bool _text_focus = false;
        public bool _popup_enter = false;
        public bool _popup_focus = false;

        public event Action<string> SearchRequest;

        public bool ShowHide
        {
            get { return (bool)GetValue(ShowHideProperty); }
            set { SetValue(ShowHideProperty, value); }
        }

        public static readonly DependencyProperty ShowHideProperty =
            DependencyProperty.Register("ShowHide", typeof(bool), typeof(SearchInput), new PropertyMetadata(false));

        private void UpdateShowHide()
        {
            var p = ShowHide;
            ShowHide = _mouse_enter || _text_focus || _popup_enter || _popup_focus;
            //Log.Debug($"_mouse_enter={_mouse_enter} _text_focus={_text_focus} _popup_enter={_popup_enter} _popup_focus={_popup_focus} : {ShowHide}");

            if (p != ShowHide)
            {
                if (ShowHide)
                    ShowInputAction.Begin();
                else
                    HideInputAction.Begin();
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            _text_focus = true;
            UpdateShowHide();
        }

        private void MainPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            _mouse_enter = true;
            UpdateShowHide();
        }

        private void MainPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            _mouse_enter = false;
            UpdateShowHide();
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _text_focus = false;
            UpdateShowHide();
        }

        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!SearchTagGallery.SupportFeatures.HasFlag(GallerySupportFeature.TagSearch))
                return;

            if (picking_imcomplete_string_frag == null)
            {
                picking_imcomplete_string_frag = string.Empty;
                return;
            }

            var input_string_frags = Input.Text.Split(' ');

            var diff_frags = input_string_frags.Except(prev_store_string_frags);

            picking_imcomplete_string_frag = diff_frags.LastOrDefault() ?? string.Empty;

            InputChanged(picking_imcomplete_string_frag);

            prev_store_string_frags = input_string_frags;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Input.Text))
                return;

            SearchRequest?.Invoke(Input.Text);
        }

        private void Input_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                Button_Click(null, null);
            }
            else if (e.Key == Key.Down)
            {
                SuggestList.Focus();
            }
        }

        #region Suggest Input Processor

        IEnumerable<string> prev_store_string_frags = Enumerable.Empty<string>();
        Task suggest_query_task = null;
        DateTime update_input_time;
        Tag[] cached_suggests;
        string query_string = null;
        string picking_imcomplete_string_frag = string.Empty;

        private void TextBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void ComfirmTag(string tag_name)
        {
            var picking_string = picking_imcomplete_string_frag;

            CancelSuggest();

            Logger.Debug($"Tag comfirmed : {picking_string} -> {tag_name}");

            Input.Text = string.Join(" ", Input.Text.Split(' ').Select(x => picking_string == x ? tag_name : x));

            //todo : move input cursor to right place
            int pos = Input.Text.IndexOf(tag_name);
            pos = pos < 0 ? Input.Text.Length : pos + tag_name.Length;

            Input.Focus();
            Input.CaretIndex = pos;
        }

        private void SuggestBox_MouseEnter(object sender, MouseEventArgs e)
        {
            _popup_enter = true;
            UpdateShowHide();
        }

        private void SuggestBox_MouseLeave(object sender, MouseEventArgs e)
        {
            _popup_enter = false;
            UpdateShowHide();
        }

        private void TextBlock_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if ((sender as ListBox).SelectedItem is Tag tag)
                {
                    var tag_name = tag.Name;

                    ComfirmTag(tag_name);
                }
            }
            else if (e.Key != Key.Up && e.Key != Key.Down)
            {
                Input.Focus();
                Input.RaiseEvent(e);
                CancelSuggest();
            }
        }

        private void CancelSuggest()
        {
            Logger.Debug($"clean query_string");

            SuggestBox.IsOpen = false;
            cached_suggests = null;

            picking_imcomplete_string_frag = null;

            query_string = null;
        }

        private void SuggestList_GotFocus(object sender, RoutedEventArgs e)
        {
            _popup_focus = true;
            UpdateShowHide();
        }

        private void SuggestList_LostFocus(object sender, RoutedEventArgs e)
        {
            _popup_focus = false;
            UpdateShowHide();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var tag_name = (sender as Button).Content.ToString();

            ComfirmTag(tag_name);
        }

        public async void InputChanged(string input_imcomplete_words)
        {
            Logger.Debug($"input_imcomplete_words={input_imcomplete_words}");

            update_input_time = DateTime.Now;

            if (string.IsNullOrWhiteSpace(input_imcomplete_words) || (query_string != null && !input_imcomplete_words.StartsWith(query_string)))
            {
                CancelSuggest();
                return;
            }

            var cur_gallery = SearchTagGallery;

            if (cached_suggests == null)
            {
                if (suggest_query_task == null)
                {
                    while (true)
                    {
                        var time_past = DateTime.Now - update_input_time;

                        if (time_past.TotalMilliseconds >= 1000)
                            break;
                        Thread.Sleep(500);
                    }

                    input_imcomplete_words = picking_imcomplete_string_frag;

                    Logger.Debug($"Start search tag. input_imcomplete_words={input_imcomplete_words}");

                    if (!string.IsNullOrWhiteSpace(input_imcomplete_words))
                    {
                        Suggests.Clear();
                        SuggestBox.IsOpen = true;

                        var lock_string = query_string = input_imcomplete_words;

                        using (LoadingStatus.BeginBusy("Tag searching..."))
                        {
                            var list = cur_gallery
                            .Feature<IGalleryTagSearch>()
                            .SearchTagAsync(input_imcomplete_words)
                            .OrderBy(tag => tag.Name.IndexOf(input_imcomplete_words));

                            var take_count = SettingManager.LoadSetting<GlobalSetting>().MaxSearchSuggestsCount;
                            cached_suggests = await (take_count == 0 ? list : list.Take(take_count)).ToArrayAsync();

                            Logger.Debug($"Got {cached_suggests.Length} tags.");
                        }

                        if (lock_string == query_string)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                foreach (var item in cached_suggests)
                                    Suggests.Add(item);
                            });
                        }
                    }
                }
            }
            else
            {
                Suggests.Clear();

                foreach (var item in cached_suggests.Where(x => x.Name.Contains(input_imcomplete_words)).OrderBy(tag => tag.Name.IndexOf(input_imcomplete_words)))
                    Suggests.Add(item);

                SuggestBox.IsOpen = true;

                Logger.Debug($"get suggestions immediatly from cache");
            }
        }

        #endregion
    }
}
