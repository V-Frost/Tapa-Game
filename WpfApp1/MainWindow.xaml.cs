using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;


namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private int GridSize = 6;
        private Button[,] buttons;
        private List<int>[,] puzzleHints;
        private readonly int[] dRow = { -1, 1, 0, 0 };
        private readonly int[] dCol = { 0, 0, -1, 1 };
        private bool highlightRowAndColumn = false;
        public bool highlightErrorsInBlue;
        private Button? lastColoredButton = null;  
        private bool highlightLastChange = false;
        private SettingsWindow? settingsWindow = null;
        private bool nightModeEnabled = false;

        private TimeSpan elapsedTime;
        private bool isPaused = false;
        DispatcherTimer gameTimer;


        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();

            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1); 
            gameTimer.Tick += GameTimer_Tick;
            elapsedTime = TimeSpan.Zero;
            StartTimer();  
        }

        private void InitializeGame()
        {
            SetGridSize();
            SetupGameGrid();
            GenerateHints(GenerateBlackCells());
            EnableNightMode(nightModeEnabled);
        }

        private void SetGridSize()
        {
            GameGrid.Children.Clear();
            GameGrid.Rows = GridSize;
            GameGrid.Columns = GridSize;
            buttons = new Button[GridSize, GridSize];
            puzzleHints = new List<int>[GridSize, GridSize];
            AdjustWindowAndCellSizes();
        }

        private void AdjustWindowAndCellSizes()
        {
            int cellSize = GridSize == 15 ? 40 : 50;
            int gridPixelSize = GridSize * cellSize;
            GameBorder.Width = gridPixelSize + 30;
            GameBorder.Height = gridPixelSize + 30;
            this.Width = gridPixelSize + 100;
            this.Height = gridPixelSize + 190;
            CenterWindow();
        }

        private void SetupGameGrid()
        {
            for (int row = 0; row < GridSize; row++)
            {
                for (int col = 0; col < GridSize; col++)
                {
                    Button button = CreateButton();
                    buttons[row, col] = button;
                    GameGrid.Children.Add(button);
                }
            }
        }

        private Button CreateButton()
        {
            var button = new Button
            {
                Foreground = Brushes.Black,
                FontSize = 14,
                Background = Brushes.White,
                Width = GridSize == 15 ? 40 : 50,
                Height = GridSize == 15 ? 40 : 50
            };

            button.Click += Cell_Click;  
            button.MouseEnter += Button_MouseEnter;  
            button.MouseLeave += Button_MouseLeave; 

            return button;
        }

        private void SettingsGameField_Click(object sender, RoutedEventArgs e)
        {
            if (settingsWindow == null || !settingsWindow.IsVisible)
            {
                settingsWindow = new SettingsWindow(this);
                settingsWindow.Left = this.Left + this.Width;  
                settingsWindow.Top = this.Top;

                settingsWindow.Closed += (s, args) => { settingsWindow = null; };

                settingsWindow.Show();
            }
            else
            {
                settingsWindow.Focus();  
            }
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            if (highlightRowAndColumn)  
            {
                Button hoveredButton = (Button)sender;
                int index = GameGrid.Children.IndexOf(hoveredButton);
                int row = index / GridSize;
                int col = index % GridSize;

                HighlightRowAndColumn(row, col); 
            }
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            if (highlightRowAndColumn) 
            {
                ClearRowColumnHighlights();  
            }
        }

        private void HighlightRowAndColumn(int row, int col)
        {
            ClearRowColumnHighlights(); 

            Brush highlightColor = nightModeEnabled ? Brushes.LightSlateGray : Brushes.LightGray;

            for (int i = 0; i < GridSize; i++)
            {
                if (buttons[row, i].Background == Brushes.White || buttons[row, i].Background.ToString() == new SolidColorBrush(Colors.Gray).ToString())
                {
                    buttons[row, i].Background = highlightColor;  
                }

                if (buttons[i, col].Background == Brushes.White || buttons[i, col].Background.ToString() == new SolidColorBrush(Colors.Gray).ToString())
                {
                    buttons[i, col].Background = highlightColor;  
                }
            }
        }

        private void ClearRowColumnHighlights()
        {
            for (int row = 0; row < GridSize; row++)
            {
                for (int col = 0; col < GridSize; col++)
                {
                    if (buttons[row, col].Background == Brushes.LightGray)
                    {
                        buttons[row, col].Background = Brushes.White;
                    }

                    if (buttons[row, col].Background == Brushes.LightSlateGray)
                    {
                        buttons[row, col].Background = Brushes.Gray;
                    }

                }
            }
        }

        private bool[,] GenerateBlackCells()
        {
            var blackCells = new bool[GridSize, GridSize];
            int totalBlackCells = (int)(GridSize * GridSize * 0.7);
            PlaceRandomBlackCells(blackCells, totalBlackCells);
            return blackCells;
        }

        private void PlaceRandomBlackCells(bool[,] blackCells, int totalBlackCells)
        {
            Random rand = new Random();
            int currentBlackCells = 0;

            while (currentBlackCells < totalBlackCells)
            {
                int row = rand.Next(0, GridSize);
                int col = rand.Next(0, GridSize);

                if (!blackCells[row, col] && !CheckFor2x2Block(row, col, blackCells))
                {
                    blackCells[row, col] = true;
                    currentBlackCells++;
                }
            }
        }

        private bool CheckFor2x2Block(int row, int col, bool[,] blackCells)
        {
            for (int i = row - 1; i <= row; i++)
            {
                for (int j = col - 1; j <= col; j++)
                {
                    if (IsValidCell(i, j) && IsValidCell(i + 1, j + 1) && Is2x2Block(i, j, blackCells))
                    {
                        if (highlightErrorsInBlue)
                        {
                            buttons[i, j].Background = Brushes.LightBlue;
                            buttons[i + 1, j].Background = Brushes.LightBlue;
                            buttons[i, j + 1].Background = Brushes.LightBlue;
                            buttons[i + 1, j + 1].Background = Brushes.LightBlue;
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private bool CheckForInvalidBlock(int row, int col, bool[,] blackCells)
        {
            if (CheckFor2x2Block(row, col, blackCells))
            {
                ColorBlock(row, col, 2, 2, Brushes.LightBlue);
                return true;
            }

            for (int height = 2; height <= blackCells.GetLength(0) - row; height++)
            {
                for (int width = 2; width <= blackCells.GetLength(1) - col; width++)
                {
                    if (IsBlock(row, col, height, width, blackCells))
                    {
                        ColorBlock(row, col, height, width, Brushes.LightBlue);
                        return true; 
                    }
                }
            }

            return false; 
        }

        private void ColorBlock(int startRow, int startCol, int height, int width, SolidColorBrush color)
        {
            for (int i = startRow; i < startRow + height; i++)
            {
                for (int j = startCol; j < startCol + width; j++)
                {
                    if (buttons[i, j].Background == Brushes.Black)
                    {
                        buttons[i, j].Background = color;  
                    }
                }
            }
        }

        private bool IsBlock(int startRow, int startCol, int height, int width, bool[,] blackCells)
        {
            for (int i = startRow; i < startRow + height; i++)
            {
                for (int j = startCol; j < startCol + width; j++)
                {
                    if (!IsValidCell(i, j) || !blackCells[i, j])  
                    {
                        return false;
                    }
                }
            }
            return true;  
        }

        private bool Is2x2Block(int row, int col, bool[,] blackCells)
        {
            return blackCells[row, col] && blackCells[row + 1, col] && blackCells[row, col + 1] && blackCells[row + 1, col + 1];
        }

        private bool IsValidCell(int row, int col)
        {
            return row >= 0 && row < GridSize && col >= 0 && col < GridSize;
        }

        private void GenerateHints(bool[,] blackCells)
        {
            int totalHints = GetHintCountBasedOnSize();
            Random rand = new Random();

            while (totalHints > 0)
            {
                int row = rand.Next(0, GridSize);
                int col = rand.Next(0, GridSize);

                if (!blackCells[row, col] && puzzleHints[row, col] == null)
                {
                    var groupSizes = GetBlackCellGroupsSizes(row, col, blackCells);
                    if (groupSizes.Any() && groupSizes.Count <= 3 && !CheckFor2x2Block(row, col, blackCells))
                    {
                        SetHint(row, col, groupSizes);
                        totalHints--;
                    }
                }
            }
        }

        private int GetHintCountBasedOnSize()
        {
            return GridSize switch
            {
                6 => 8,
                10 => 12,
                15 => 20,
                _ => 8
            };
        }

        private void SetHint(int row, int col, List<int> groupSizes)
        {
            groupSizes.Sort();
            groupSizes.Reverse();
            puzzleHints[row, col] = groupSizes;
            buttons[row, col].Content = string.Join(" ", groupSizes);
            buttons[row, col].IsEnabled = false;
        }

        private List<int> GetBlackCellGroupsSizes(int row, int col, bool[,] blackCells)
        {
            bool[,] visited = new bool[3, 3];
            List<int> groupSizes = new List<int>();

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int newRow = row + i;
                    int newCol = col + j;

                    if (IsValidCell(newRow, newCol) && (i != 0 || j != 0) && blackCells[newRow, newCol] && !visited[i + 1, j + 1])
                    {
                        int groupSize = ExploreGroupSize(newRow, newCol, row, col, blackCells, visited);
                        groupSizes.Add(groupSize);
                    }
                }
            }

            return groupSizes;
        }

        private int ExploreGroupSize(int row, int col, int centerRow, int centerCol, bool[,] blackCells, bool[,] visited)
        {
            Stack<(int, int)> stack = new Stack<(int, int)>();
            int groupSize = 0;
            int localRow = row - centerRow + 1;
            int localCol = col - centerCol + 1;
            stack.Push((localRow, localCol));
            visited[localRow, localCol] = true;

            while (stack.Count > 0)
            {
                var (currentLocalRow, currentLocalCol) = stack.Pop();
                groupSize++;
                int currentRow = centerRow + currentLocalRow - 1;
                int currentCol = centerCol + currentLocalCol - 1;

                for (int k = 0; k < 4; k++)
                {
                    int newRow = currentRow + dRow[k];
                    int newCol = currentCol + dCol[k];
                    int newLocalRow = currentLocalRow + dRow[k];
                    int newLocalCol = currentLocalCol + dCol[k];

                    if (newLocalRow >= 0 && newLocalRow <= 2 && newLocalCol >= 0 && newLocalCol <= 2)
                    {
                        if (IsValidCell(newRow, newCol) && blackCells[newRow, newCol] && !visited[newLocalRow, newLocalCol])
                        {
                            stack.Push((newLocalRow, newLocalCol));
                            visited[newLocalRow, newLocalCol] = true;
                        }
                    }
                }
            }

            return groupSize;
        }

        private Button? lastClickedButton = null; 
        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            int index = GameGrid.Children.IndexOf(clickedButton);
            int row = index / GridSize;
            int col = index % GridSize;

            if (highlightLastChange)
            {
                if (lastClickedButton != null)
                {
                    if (nightModeEnabled)
                    {
                        lastClickedButton.BorderBrush = Brushes.Gray;
                        lastClickedButton.BorderThickness = new Thickness(1);  
                    }
                    else
                    {
                        lastClickedButton.BorderBrush = Brushes.Black;
                        lastClickedButton.BorderThickness = new Thickness(1); 

                    }                    
                }

                clickedButton.BorderBrush = Brushes.Blue;
                clickedButton.BorderThickness = new Thickness(2);

                lastClickedButton = clickedButton;
            }


            if (clickedButton.Background == Brushes.White || clickedButton.Background == Brushes.LightGray || clickedButton.Background.ToString() == new SolidColorBrush(Colors.Gray).ToString())
            {
                clickedButton.Background = Brushes.Black;  

                if (CheckFor2x2Block(row, col, GetCurrentBlackCells()))
                {
                    if (highlightErrorsInBlue)
                    {
                        ColorBlock(row, col, 2, 2, Brushes.LightBlue);
                        return; 
                    }
                    else
                    {
                        clickedButton.Background = Brushes.White;  
                        MessageBox.Show("Зафарбовувати клітинки 2х2 - Заборонено.");
                        return;  
                    }
                }
            }
            else if (clickedButton.Background == Brushes.Black || clickedButton.Background == Brushes.LightBlue)
            {
                if (nightModeEnabled)
                {
                    clickedButton.Background = new SolidColorBrush(Colors.Gray); 
                }
                else
                {
                    clickedButton.Background = Brushes.White; 
                }
            }
        }

        private bool[,] GetCurrentBlackCells()
        {
            var blackCells = new bool[GridSize, GridSize];

            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    if (buttons[i, j].Background == Brushes.Black)
                    {
                        blackCells[i, j] = true;
                    }
                }
            }

            return blackCells;
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            if (GridSizeComboBox.SelectedItem is ComboBoxItem selectedItem && int.TryParse(selectedItem.Content.ToString(), out int newSize))
            {
                GridSize = newSize;
                InitializeGame();
                ResetTimer();
            }
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsPuzzleCorrect() && CheckBlackCellConnectivity(GetCurrentBlackCells()))
            {
                MessageBox.Show("Правильно!");
                gameTimer.Stop();

                TimerTextBlock.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                MessageBox.Show("Неправильно, спробуйте ще раз!");
            }
        }

        private bool IsPuzzleCorrect()
        {
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    if (puzzleHints[i, j] != null)
                    {
                        List<int> expectedGroupSizes = puzzleHints[i, j];
                        List<int> actualGroupSizes = GetUserBlackCellGroupsSizes(i, j);

                        expectedGroupSizes.Sort();
                        actualGroupSizes.Sort();

                        if (!expectedGroupSizes.SequenceEqual(actualGroupSizes))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private List<int> GetUserBlackCellGroupsSizes(int row, int col)
        {
            bool[,] visited = new bool[3, 3];
            List<int> groupSizes = new List<int>();

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int newRow = row + i;
                    int newCol = col + j;

                    if (IsValidCell(newRow, newCol) && (i != 0 || j != 0) && buttons[newRow, newCol].Background == Brushes.Black && !visited[i + 1, j + 1])
                    {
                        int groupSize = ExploreUserGroupSize(newRow, newCol, row, col, visited);
                        groupSizes.Add(groupSize);
                    }
                }
            }

            return groupSizes;
        }

        private int ExploreUserGroupSize(int row, int col, int centerRow, int centerCol, bool[,] visited)
        {
            Stack<(int, int)> stack = new Stack<(int, int)>();
            int groupSize = 0;
            int localRow = row - centerRow + 1;
            int localCol = col - centerCol + 1;
            stack.Push((localRow, localCol));
            visited[localRow, localCol] = true;

            while (stack.Count > 0)
            {
                var (currentLocalRow, currentLocalCol) = stack.Pop();
                groupSize++;
                int currentRow = centerRow + currentLocalRow - 1;
                int currentCol = centerCol + currentLocalCol - 1;

                for (int k = 0; k < 4; k++)
                {
                    int newRow = currentRow + dRow[k];
                    int newCol = currentCol + dCol[k];
                    int newLocalRow = currentLocalRow + dRow[k];
                    int newLocalCol = currentLocalCol + dCol[k];

                    if (newLocalRow >= 0 && newLocalRow <= 2 && newLocalCol >= 0 && newLocalCol <= 2)
                    {
                        if (IsValidCell(newRow, newCol) && buttons[newRow, newCol].Background == Brushes.Black && !visited[newLocalRow, newLocalCol])
                        {
                            stack.Push((newLocalRow, newLocalCol));
                            visited[newLocalRow, newLocalCol] = true;
                        }
                    }
                }
            }

            return groupSize;
        }

        private void CenterWindow()
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            this.Left = (screenWidth - this.Width) / 2;
            this.Top = (screenHeight - this.Height) / 2;
        }

        private bool CheckBlackCellConnectivity(bool[,] blackCells)
        {
            bool[,] visited = new bool[GridSize, GridSize];
            List<(int, int)> blackCellsList = GetBlackCells(blackCells);

            if (blackCellsList.Count == 0) return false;

            Stack<(int, int)> stack = new Stack<(int, int)>();
            stack.Push(blackCellsList[0]);
            visited[blackCellsList[0].Item1, blackCellsList[0].Item2] = true;
            int connectedCount = 1;

            while (stack.Count > 0)
            {
                var (currentRow, currentCol) = stack.Pop();
                for (int k = 0; k < 4; k++)
                {
                    int newRow = currentRow + dRow[k];
                    int newCol = currentCol + dCol[k];

                    if (IsValidCell(newRow, newCol) && !visited[newRow, newCol] && blackCells[newRow, newCol])
                    {
                        stack.Push((newRow, newCol));
                        visited[newRow, newCol] = true;
                        connectedCount++;
                    }
                }
            }

            return connectedCount == blackCellsList.Count;
        }

        private List<(int, int)> GetBlackCells(bool[,] blackCells)
        {
            var blackCellsList = new List<(int, int)>();
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    if (blackCells[i, j])
                    {
                        blackCellsList.Add((i, j));
                    }
                }
            }
            return blackCellsList;
        }

        public void EnableLastChangeHighlighting(bool isEnabled)
        {
            highlightLastChange = isEnabled;
        }

        public void EnableRowColumnHighlighting(bool isEnabled)
        {
            highlightRowAndColumn = isEnabled;
            if (!isEnabled)
            {
                ClearRowColumnHighlights();  
            }
        }

        public void EnableNightMode(bool isEnabled)
        {
            nightModeEnabled = isEnabled;

            if (isEnabled)
            {
                this.Background = new SolidColorBrush(Colors.DarkSlateGray);
                GameBorder.Background = new SolidColorBrush(Colors.DarkSlateGray);  

                foreach (Button button in buttons)
                {
                    button.Foreground = new SolidColorBrush(Colors.Black); 
                    if (button.Background == Brushes.White)  
                    {
                        button.Background = new SolidColorBrush(Colors.Gray); 
                    }
                }
                TopMenu.Background = new SolidColorBrush(Colors.DarkSlateGray);


            }
            else
            {
                this.Background = new SolidColorBrush(Colors.White);
                GameBorder.Background = new SolidColorBrush(Colors.White);  
                GameGrid.Background = new SolidColorBrush(Colors.White); 

                foreach (Button button in buttons)
                {
                    button.Foreground = new SolidColorBrush(Colors.Black);

                    if (button.Background is SolidColorBrush solidColorBrush && solidColorBrush.Color == Colors.Gray)
                    {
                        button.Background = Brushes.White;  
                    }
                }

                TopMenu.Background = new SolidColorBrush(Colors.White);
            }

        }

        public void EnableErrorHighlighting(bool isEnabled)
        {
            highlightErrorsInBlue = isEnabled;  
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (isPaused)
            {
                StartTimer();
                PauseButton.Content = "| |";
                EnableGameButtons(true); 
            }
            else
            {
                PauseTimer();
                PauseButton.Content = "▶️";
                EnableGameButtons(false);  
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (!isPaused)
            {
                elapsedTime = elapsedTime.Add(TimeSpan.FromSeconds(1));
                TimerTextBlock.Text = elapsedTime.ToString(@"mm\:ss");
            }
        }
   
        private void StartTimer()
        {
            gameTimer.Start();
            isPaused = false;
        }

        private void PauseTimer()
        {
            isPaused = true;
        }

        private void ResetTimer()
        {
            elapsedTime = TimeSpan.Zero;
            TimerTextBlock.Text = "00:00";
            StartTimer();
        }
      
        private void EnableGameButtons(bool isEnabled)
        {
            foreach (Button button in buttons)
            {
                button.IsEnabled = isEnabled;
            }
        }

    }
}
