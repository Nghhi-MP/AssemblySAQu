using SAQu;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WarningManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class Suggestion : INotifyPropertyChanged
        {
            private string _contents = string.Empty;
            private int _votes = 0;

            public required string Contents
            {
                get => _contents;
                set
                {
                    if (_contents != value)
                    {
                        _contents = value;
                        OnPropertyChanged(nameof(Contents));
                    }
                }
            }

            public int Votes
            {
                get => _votes;
                set
                {
                    if (_votes != value)
                    {
                        _votes = value;
                        OnPropertyChanged(nameof(Votes));
                    }
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ObservableCollection<Suggestion> Suggestions { get; set; } = [];

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }
        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)suggestionContentsField.Template.FindName("PART_EditableTextBox", suggestionContentsField);
            if (textBox != null)
            {
                textBox.TextChanged += ComboBoxTextChanged;
            }
        }

        private void SelectItemInBox()
        {
            string suggestionContents = suggestionContentsField.Text;

            if (Suggestions.FirstOrDefault(suggestion => suggestion.Contents == suggestionContents) is { } suggestion)
            {
                int i = suggestionsGrid.Items.IndexOf(suggestion);
                suggestionsGrid.SelectedIndex = i;
                suggestionsGrid.ScrollIntoView(suggestion);
            }
        }

        private void ComboBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            SelectItemInBox();
        }

        private void OrderSuggestions()
        {
            var ordered = Suggestions.OrderByDescending(suggestion => suggestion.Votes);
            for (int i = 0; i < ordered.Count(); i++)
            {
                var oldIndex = Suggestions.IndexOf(ordered.ElementAt(i));
                if (oldIndex != i)
                    Suggestions.Move(oldIndex, i);
            }
        }

        private void ComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            string suggestionContents = suggestionContentsField.Text;

            switch (e.Key)
            {
                case Key.F2:
                    {
                        if (Suggestions.FirstOrDefault(suggestion => suggestion.Contents == suggestionContents) is Suggestion suggestion)
                        {
                            int indexOfSuggestion = Suggestions.IndexOf(suggestion);
                            if (indexOfSuggestion < Suggestions.Count - 1)
                            {
                                Suggestions.Move(indexOfSuggestion, indexOfSuggestion + 1);
                            }
                        }
                    }
                    break;
                case Key.F3:
                    {
                        if (Suggestions.FirstOrDefault(suggestion => suggestion.Contents == suggestionContents) is Suggestion suggestion)
                        {
                            int indexOfSuggestion = Suggestions.IndexOf(suggestion);
                            if (indexOfSuggestion != 0)
                            {
                                Suggestions.Move(indexOfSuggestion, indexOfSuggestion - 1);
                            }
                        }
                    }
                    break;
                case Key.Enter:
                    {
                        if (string.IsNullOrWhiteSpace(suggestionContents)) return;

                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            if (Suggestions.FirstOrDefault(suggestion => suggestion.Contents == suggestionContents) is Suggestion suggestion)
                            {
                                int suggestionIndex = Suggestions.IndexOf(suggestion);
                                Suggestions.Remove(suggestion);
                                //MessageBox.Show($"{suggestionIndex} < {Suggestions.Count}");
                                if (suggestionIndex >= 0 && suggestionIndex < Suggestions.Count)
                                    suggestionContentsField.Text = Suggestions[suggestionIndex].Contents;
                                else if (Suggestions.Count < 1)
                                    suggestionContentsField.Text = string.Empty;
                                else
                                    suggestionContentsField.Text = Suggestions[^1].Contents;
                            }
                        }
                        else
                        {
                            if (Suggestions.FirstOrDefault(suggestion => suggestion.Contents == suggestionContents) is Suggestion suggestion)
                            {
                                var inputBox = new InputBox()
                                {
                                    Title = "Vote"
                                };
                                inputBox.CaptionBlock.Text = "Enter number of votes:";
                                inputBox.InputField.Text = suggestion.Votes.ToString();
                                inputBox.EnterButton.Click += (sender, args) =>
                                {
                                    if (int.TryParse(inputBox.InputField.Text, out int value) && value >= 0)
                                    {
                                        suggestion.Votes = value;
                                        OrderSuggestions();
                                    }
                                };
                                inputBox.ShowDialog();
                            }
                            else
                            {
                                Suggestions.Add(new() { Contents = suggestionContents });
                                if (suggestionContentsField.Template.FindName("PART_EditableTextBox", suggestionContentsField)
                                    is TextBox textBox)
                                {
                                    textBox.SelectAll();
                                }
                                suggestionContentsField.SelectedIndex = Suggestions.Count - 1;
                            }
                        }
                    }
                    SelectItemInBox();
                    break;
            }

        }

        private void suggestionsGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            suggestionCol.Width = Width / 2;
            votesCol.Width = Width / 2;
        }
    }
}