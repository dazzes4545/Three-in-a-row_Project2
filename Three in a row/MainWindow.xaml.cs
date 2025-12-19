using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Threading.Tasks;

namespace MatchThreeGame
{
    public partial class MainWindow : Window
    {
        int stroka = 5;
        int stolby = 5;
        int rezmetka = 70;
        int schet = 0;
        int schetRandom = 0;

        Color[] cveta = {
            Colors.Red,
            Colors.Blue,
            Colors.Green,
            Colors.Yellow,
            Colors.Purple
        };

        Figurka[,] pole;

        bool pervayaVibrana = false;
        int pervayaStroka = -1;
        int perviyStolbets = -1;
        int vtorayaStroka = -1;
        int vtoroyStolbets = -1;

        bool animaciaIdet = false;

        Random sluchayniy = new Random();

        public MainWindow()
        {
            InitializeComponent();
            NachatNovuyuIgru();
        }

        class Figurka
        {
            public int cvet;
            public Rectangle pryamougolnik;
        }

        void NachatNovuyuIgru()
        {
            schet = 0;
            ObnovitSchet();

            schetRandom = sluchayniy.Next(100, 501);
            TargetScoreText.Text = schetRandom.ToString();

            MessageText.Text = "";
            GameCanvas.Children.Clear();
            pervayaVibrana = false;
            animaciaIdet = false;

            pole = new Figurka[stroka, stolby];

            for (int i = 0; i < stroka; i++)
            {
                for (int j = 0; j < stolby; j++)
                {
                    SdelatFigurku(i, j);
                }
            }

            UbratNachalnieSovpadeniya();
        }

        void SdelatFigurku(int stroka, int stolbets)
        {
            Figurka figurka = new Figurka();
            figurka.cvet = sluchayniy.Next(cveta.Length);

            Rectangle pryamougolnik = new Rectangle();
            pryamougolnik.Width = rezmetka - 6;
            pryamougolnik.Height = rezmetka - 6;
            pryamougolnik.Stroke = Brushes.Black;
            pryamougolnik.StrokeThickness = 2;
            pryamougolnik.RadiusX = 8;
            pryamougolnik.RadiusY = 8;

            SolidColorBrush kirpich = new SolidColorBrush();
            kirpich.Color = cveta[figurka.cvet];
            pryamougolnik.Fill = kirpich;

            Canvas.SetLeft(pryamougolnik, stolbets * rezmetka + 3);
            Canvas.SetTop(pryamougolnik, stroka * rezmetka + 3);

            figurka.pryamougolnik = pryamougolnik;
            pole[stroka, stolbets] = figurka;

            GameCanvas.Children.Add(pryamougolnik);
        }

        void UbratNachalnieSovpadeniya()
        {
            List<Figurka> sovpadeniya = NaytiVseSovpadeniya();

            while (sovpadeniya.Count > 0)
            {
                foreach (Figurka figurka in sovpadeniya)
                {
                    figurka.cvet = sluchayniy.Next(cveta.Length);
                    ObnovitCvetFigurki(figurka);
                }
                sovpadeniya = NaytiVseSovpadeniya();
            }
        }

        void ObnovitCvetFigurki(Figurka figurka)
        {
            if (figurka == null || figurka.pryamougolnik == null) return;

            SolidColorBrush kirpich = new SolidColorBrush();
            kirpich.Color = cveta[figurka.cvet];
            figurka.pryamougolnik.Fill = kirpich;
        }

        void GameCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (animaciaIdet) return;

            Point tochkaKlika = e.GetPosition(GameCanvas);
            int stolbets = (int)(tochkaKlika.X / rezmetka);
            int stroka = (int)(tochkaKlika.Y / rezmetka);

            if (stroka < 0 || stroka >= this.stroka || stolbets < 0 || stolbets >= stolby) return;

            if (!pervayaVibrana)
            {
                pervayaStroka = stroka;
                perviyStolbets = stolbets;
                pole[stroka, stolbets].pryamougolnik.Stroke = Brushes.White;
                pole[stroka, stolbets].pryamougolnik.StrokeThickness = 4;
                pervayaVibrana = true;
            }
            else
            {
                vtorayaStroka = stroka;
                vtoroyStolbets = stolbets;

                pole[pervayaStroka, perviyStolbets].pryamougolnik.Stroke = Brushes.Black;
                pole[pervayaStroka, perviyStolbets].pryamougolnik.StrokeThickness = 2;

                if (MojnoPomenyat(pervayaStroka, perviyStolbets, vtorayaStroka, vtoroyStolbets))
                {
                    PomenyatFigurki(pervayaStroka, perviyStolbets, vtorayaStroka, vtoroyStolbets);
                }
                else
                {
                    pervayaStroka = vtorayaStroka;
                    perviyStolbets = vtoroyStolbets;
                    pole[pervayaStroka, perviyStolbets].pryamougolnik.Stroke = Brushes.White;
                    pole[pervayaStroka, perviyStolbets].pryamougolnik.StrokeThickness = 4;
                }
            }
        }

        bool MojnoPomenyat(int s1, int st1, int s2, int st2)
        {
            int raznicaStrok = Math.Abs(s1 - s2);
            int raznicaStolbtsov = Math.Abs(st1 - st2);

            return (raznicaStrok == 1 && raznicaStolbtsov == 0) || (raznicaStrok == 0 && raznicaStolbtsov == 1);
        }

        async void PomenyatFigurki(int s1, int st1, int s2, int st2)
        {
            animaciaIdet = true;
            pervayaVibrana = false;

            Figurka vremennaya = pole[s1, st1];
            pole[s1, st1] = pole[s2, st2];
            pole[s2, st2] = vremennaya;

            await PodvinutFigurku(pole[s1, st1].pryamougolnik, st1, s1);
            await PodvinutFigurku(pole[s2, st2].pryamougolnik, st2, s2);

            List<Figurka> sovpadeniya = NaytiVseSovpadeniya();

            if (sovpadeniya.Count > 0)
            {
                UdalitSovpadeniya(sovpadeniya);
                await ZapolnitPole();

                sovpadeniya = NaytiVseSovpadeniya();
                while (sovpadeniya.Count > 0)
                {
                    UdalitSovpadeniya(sovpadeniya);
                    await ZapolnitPole();
                    sovpadeniya = NaytiVseSovpadeniya();
                }

                if (schet >= schetRandom)
                {
                    MessageText.Text = "ПОБЕДА! Цель достигнута!";
                }
            }
            else
            {
                vremennaya = pole[s1, st1];
                pole[s1, st1] = pole[s2, st2];
                pole[s2, st2] = vremennaya;

                await PodvinutFigurku(pole[s1, st1].pryamougolnik, st1, s1);
                await PodvinutFigurku(pole[s2, st2].pryamougolnik, st2, s2);

                MessageText.Text = "Нет совпадений!";
            }

            animaciaIdet = false;
        }

        Task PodvinutFigurku(Rectangle pryamougolnik, int stolbets, int stroka)
        {
            var zadacha = new TaskCompletionSource<bool>();

            var animaciaX = new System.Windows.Media.Animation.DoubleAnimation();
            animaciaX.To = stolbets * rezmetka + 3;
            animaciaX.Duration = TimeSpan.FromMilliseconds(200);

            var animaciaY = new System.Windows.Media.Animation.DoubleAnimation();
            animaciaY.To = stroka * rezmetka + 3;
            animaciaY.Duration = TimeSpan.FromMilliseconds(200);

            animaciaX.Completed += (s, e) => zadacha.SetResult(true);

            pryamougolnik.BeginAnimation(Canvas.LeftProperty, animaciaX);
            pryamougolnik.BeginAnimation(Canvas.TopProperty, animaciaY);

            return zadacha.Task;
        }

        List<Figurka> NaytiVseSovpadeniya()
        {
            List<Figurka> resultat = new List<Figurka>();

            for (int i = 0; i < stroka; i++)
            {
                for (int j = 0; j < stolby - 2; j++)
                {
                    if (pole[i, j] == null || pole[i, j + 1] == null || pole[i, j + 2] == null)
                        continue;

                    if (pole[i, j].cvet == pole[i, j + 1].cvet &&
                        pole[i, j + 1].cvet == pole[i, j + 2].cvet)
                    {
                        if (!resultat.Contains(pole[i, j])) resultat.Add(pole[i, j]);
                        if (!resultat.Contains(pole[i, j + 1])) resultat.Add(pole[i, j + 1]);
                        if (!resultat.Contains(pole[i, j + 2])) resultat.Add(pole[i, j + 2]);
                    }
                }
            }

            for (int j = 0; j < stolby; j++)
            {
                for (int i = 0; i < stroka - 2; i++)
                {
                    if (pole[i, j] == null || pole[i + 1, j] == null || pole[i + 2, j] == null)
                        continue;

                    if (pole[i, j].cvet == pole[i + 1, j].cvet &&
                        pole[i + 1, j].cvet == pole[i + 2, j].cvet)
                    {
                        if (!resultat.Contains(pole[i, j])) resultat.Add(pole[i, j]);
                        if (!resultat.Contains(pole[i + 1, j])) resultat.Add(pole[i + 1, j]);
                        if (!resultat.Contains(pole[i + 2, j])) resultat.Add(pole[i + 2, j]);
                    }
                }
            }

            return resultat;
        }

        void UdalitSovpadeniya(List<Figurka> sovpadeniya)
        {
            foreach (Figurka figurka in sovpadeniya)
            {
                figurka.pryamougolnik.Fill = Brushes.Transparent;
                figurka.pryamougolnik.Stroke = Brushes.Transparent;

                schet += 10;
                ObnovitSchet();
            }
        }

        async Task ZapolnitPole()
        {
            await Task.Delay(300);

            for (int j = 0; j < stolby; j++)
            {
                int pustieMesta = 0;

                for (int i = stroka - 1; i >= 0; i--)
                {
                    if (pole[i, j] == null || pole[i, j].pryamougolnik.Fill == Brushes.Transparent)
                    {
                        pustieMesta++;
                    }
                    else if (pustieMesta > 0)
                    {
                        Figurka figurka = pole[i, j];
                        pole[i, j] = pole[i + pustieMesta, j];
                        pole[i + pustieMesta, j] = figurka;

                        await PodvinutFigurku(pole[i + pustieMesta, j].pryamougolnik, j, i + pustieMesta);
                    }
                }

                for (int k = 0; k < pustieMesta; k++)
                {
                    int stroka = k;

                    if (pole[stroka, j] == null || pole[stroka, j].pryamougolnik.Fill == Brushes.Transparent)
                    {
                        SdelatFigurku(stroka, j);
                        Canvas.SetTop(pole[stroka, j].pryamougolnik, -rezmetka);
                        await PodvinutFigurku(pole[stroka, j].pryamougolnik, j, stroka);
                    }
                }
            }
        }

        void ObnovitSchet()
        {
            ScoreText.Text = schet.ToString();

            if (schet >= schetRandom)
            {
                ScoreText.Foreground = Brushes.Green;
            }
            else if (schet >= schetRandom * 0.7)
            {
                ScoreText.Foreground = Brushes.Orange;
            }
            else
            {
                ScoreText.Foreground = Brushes.Black;
            }
        }

        void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            NachatNovuyuIgru();
        }
    }
}
//dsfsd