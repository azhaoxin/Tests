using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;

namespace NumPuzzle
{
    public partial class MainWindowVM : ObservableObject
    {
        [ObservableProperty]
        private int size = 3;
        [ObservableProperty]
        private int targetSize = 3;
        [ObservableProperty]
        private int currStep;
        [ObservableProperty]
        private int record;
        public ObservableCollection<Piece> Pieces { get; set; } = new ObservableCollection<Piece>();
        private DispatcherTimer timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1) };
        [ObservableProperty]
        private int seconds;

        [ObservableProperty]
        private bool canSelectPiece = true;
        public void InitTimer()
        {
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            Seconds++;
        }

        partial void OnSizeChanged(int value)
        {
            LoadRecord(size);
        }


        [RelayCommand]
        public void StartNew()
        {
            Pieces.Clear();
            Size = TargetSize;
            var list = Enumerable.Range(1, targetSize * targetSize - 1).OrderBy(n => Guid.NewGuid());
            Pieces.Add(new Piece { Num = 0, IsNull = true });
            foreach (var item in list)
            {
                Pieces.Add(new Piece { Num = item, IsNull = false });
            }
            if (Size % 2 == 1 && GetInverseNum() % 2 == 1)
            {
                //n为奇数不可能改变逆序数奇偶性
                SwapPiece(Pieces[1], Pieces[2]);
            }
            SetSelectedPiece(Pieces[1]);
            CurrStep = 0;
            Seconds = 0;
        }
        [RelayCommand(CanExecute = nameof(CanSelectPiece))]
        private void SelectPiece(Piece piece)
        {
            if (!piece.IsNull)
            {
                SetSelectedPiece(piece);
            }
            //MessageBox.Show("!");
        }

        private void SetSelectedPiece(Piece piece)
        {
            foreach (var item in Pieces)
            {
                item.IsSelected = false;
            }
            piece.IsSelected = true;
        }

        private bool CanMove(Piece piece)
        {
            bool result = false;
            var piecePos = Pieces.ToList().FindIndex(p => p.Num == piece.Num);
            //var nullPos = Pieces.ToList().FindIndex(p => p.IsNull);
            //var nullRow = nullPos / Size;
            //var nullCol = nullPos % Size;
            var nullPiece = Pieces.ToList().Find(p => p.IsNull);
            var up = GetUpIndex(nullPiece);
            var down = GetDownIndex(nullPiece);
            var left = GetLeftIndex(nullPiece);
            var right = GetRightIndex(nullPiece);
            if (piecePos == up || piecePos == down || piecePos == left || piecePos == right)
            {
                result = true;
            }
            return result;
        }
        private int GetUpIndex(Piece piece)
        {
            var piecePos = Pieces.ToList().FindIndex(p => p.Num == piece.Num);
            return piecePos - Size >= 0 ? piecePos - Size : -1;
        }
        private int GetDownIndex(Piece piece)
        {
            var piecePos = Pieces.ToList().FindIndex(p => p.Num == piece.Num);
            return piecePos + Size < Pieces.Count ? piecePos + Size : -1;
        }
        private int GetLeftIndex(Piece piece)
        {
            var piecePos = Pieces.ToList().FindIndex(p => p.Num == piece.Num);
            return (piecePos - 1 >= 0) && (piecePos % Size != 0) ? piecePos - 1 : -1;
        }
        private int GetRightIndex(Piece piece)
        {
            var piecePos = Pieces.ToList().FindIndex(p => p.Num == piece.Num);
            return (piecePos + 1 < Pieces.Count) && ((piecePos + 1) % Size != 0) ? piecePos + 1 : -1;
        }

        [RelayCommand]
        private void KeyUp()
        {
            KeyInput(GetUpIndex);
        }
        [RelayCommand]
        private void KeyDown()
        {
            KeyInput(GetDownIndex);
        }
        [RelayCommand]
        private void KeyLeft()
        {
            KeyInput(GetLeftIndex);
        }
        [RelayCommand]
        private void KeyRight()
        {
            KeyInput(GetRightIndex);
        }
        private void KeyInput(Func<Piece, int> func)
        {
            var selectdPiece = Pieces.ToList().Find(p => p.IsSelected);
            if (selectdPiece != null)
            {
                var selectdPiecePos = Pieces.IndexOf(selectdPiece);
                var pos = func(selectdPiece);
                if (pos != -1)
                {
                    if (Pieces[pos].IsNull)
                    {
                        Pieces[pos].Num = selectdPiece.Num;
                        Pieces[pos].IsNull = false;
                        SetSelectedPiece(Pieces[pos]);

                        Pieces[selectdPiecePos].Num = 0;
                        Pieces[selectdPiecePos].IsNull = true;
                        CurrStep++;
                    }
                    else
                    {
                        SetSelectedPiece(Pieces[pos]);
                    }
                }
            }
            if (GetInverseNum() == 0 && Pieces[Pieces.Count - 1].IsNull)
            {
                if (MessageBox.Show("牛B！是否重新开始？", "完成！", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    SaveRecord(Size);
                    StartNew();
                }

            }
        }
        //获取逆序数
        private int GetInverseNum()
        {
            int result = 0;
            for (int i = 1; i < Pieces.Count; i++)
            {
                if (Pieces[i].IsNull)
                {
                    continue;
                }
                for (int j = 0; j < i; j++)
                {
                    if (Pieces[j].IsNull)
                    {
                        continue;
                    }
                    if (Pieces[j].Num > Pieces[i].Num)
                    {
                        result++;
                    }
                }
            }
            return result;
        }
        private void SwapPiece(Piece piece1, Piece piece2)
        {
            var tempPiece = new Piece { IsNull = piece1.IsNull, IsSelected = piece1.IsSelected, Num = piece1.Num };
            piece1.IsNull = piece2.IsNull;
            //piece1.IsSelected = piece2.IsSelected;
            piece1.Num = piece2.Num;

            piece2.IsNull = tempPiece.IsNull;
            //piece2.IsSelected = tempPiece.IsSelected;
            piece2.Num = tempPiece.Num;
        }
        [RelayCommand]
        private void SaveAll()
        {
            SaveSize();
            SaveRecord(3);
            SaveRecord(4);
            SaveRecord(5);
            SaveRecord(6);
            SaveRecord(7);
        }
        private void SaveSize()
        {
            var config = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
            try
            {
                config.AppSettings.Settings["Size"].Value = Size.ToString();
            }
            catch (Exception ex)
            {
                config.AppSettings.Settings.Add("Size", Size.ToString());
            }
            config.Save(System.Configuration.ConfigurationSaveMode.Modified);
            System.Configuration.ConfigurationManager.RefreshSection("appSettings");
        }
        private void SaveRecord(int size)
        {
            var config = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
            try
            {
                config.AppSettings.Settings[$"Size_{size}_Record"].Value = size == Size ? Record.ToString() : "0";
            }
            catch (Exception ex)
            {
                config.AppSettings.Settings.Add($"Size_{size}_Record", size == Size ? Record.ToString() : "0");
            }
            config.Save(System.Configuration.ConfigurationSaveMode.Modified);
            System.Configuration.ConfigurationManager.RefreshSection("appSettings");
        }
        private void LoadSize()
        {
            var config = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
            try
            {
                Size = int.Parse(config.AppSettings.Settings[$"Size"].Value);
            }
            catch (Exception ex)
            {

            }
        }
        private void LoadRecord(int size)
        {
            var config = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
            try
            {
                Record = int.Parse(config.AppSettings.Settings[$"Size_{Size}_Record"].Value);
            }
            catch (Exception ex)
            {

            }
        }

    }
}
