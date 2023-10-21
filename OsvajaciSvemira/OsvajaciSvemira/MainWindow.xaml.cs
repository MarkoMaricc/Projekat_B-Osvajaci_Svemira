using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace OsvajaciSvemira
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        bool lijevo, desno;
        List<Rectangle> listaZaBrisanje= new List<Rectangle>();    //lista elemenata koje treba izbrisati sa ekrana
        int napadacSlike = 0;
        int metakTimer = 0;         // brojac koji se umanjuje, vrijeme posle kojega se dodaje novi metak na ekran(tajmer)
        int metakTimerGranica = 90;  //pocetna vrijednost brojaca
        int metakTimer1 = 0;               // tajmer za metak vodje vanzemaljaca
        int brojNapadaca = 0;            
        int brzinaNapadaca = 6;         
        
        int SnagaVanzemaljca = 4;      //snaga vodje vanzemaljaca
        bool krajIgre = false;

        bool garda=false;
        bool pogodjen=false;

        DispatcherTimer timerIgre = new DispatcherTimer();    
        ImageBrush igracIzgled = new ImageBrush();
        ImageBrush napadacIzgled = new ImageBrush();


        public MainWindow()
        {
            InitializeComponent();

            timerIgre.Tick += petljaIgre;    
            timerIgre.Interval = TimeSpan.FromMilliseconds(20);  //takt igre
            timerIgre.Start();         

            igracIzgled.ImageSource = new BitmapImage(new Uri("pack://application:,,,/slike/player.png"));    //slika za igraca
            igrac.Fill = igracIzgled;

            napadacIzgled.ImageSource = new BitmapImage(new Uri("pack://application:,,,/slike/3.png"));    //slika za vodju vanzemaljaca
            napadac.Fill = napadacIzgled;

            noviCanvas.Focus();
            fabrikaNapadaca(30);    //ovim pozivom biramo broj napadaca na ekranu
        }


        private void dodajVanzemaljca()    //dodavanje vodje 
        {
            napadac.Visibility = Visibility.Visible;
            healthBar.Visibility = Visibility.Visible;
            snaga.Visibility = Visibility.Visible;
            brojNapadaca++;
        }


        

        //osnovna funkcija igrice sadrzi glavnu logiku
        private void petljaIgre(object sender, EventArgs e)
        {
           if (pogodjen) {
                 healthBar.Fill = new SolidColorBrush(Colors.Green);
                pogodjen = false;
            }

            brojNeprijatelja.Content = "broj preostalih neprijatelja:" + brojNapadaca;

            Rect igracProstor = new Rect(Canvas.GetLeft(igrac),Canvas.GetTop(igrac), igrac.Width, igrac.Height); //prostor u kome se nalazi igrac
            Rect vanzemaljacProstor = new Rect(Canvas.GetLeft(napadac), Canvas.GetTop(napadac), napadac.Width, napadac.Height);
            if (lijevo && Canvas.GetLeft(igrac) > 0)
                Canvas.SetLeft(igrac,Canvas.GetLeft(igrac)-10);     //pomjeranje igraca u lijevo
            if (desno && Canvas.GetLeft(igrac) + 80 < Application.Current.MainWindow.Width)
                Canvas.SetLeft(igrac, Canvas.GetLeft(igrac) + 10);    //pomjeranje igraca u desno

            metakTimer -= 3;
            if (metakTimer < 0)         //provjera da li je vrijeme da se doda novi metak
            {
                fabrikaMetakaNapadaca(Canvas.GetLeft(igrac),10);    //postavljanje novog metka na pocetnu lokaciju
                metakTimer = metakTimerGranica;                      
            }



            if (garda == true)    //uredjivanje kretanja vodje vanzemaljaca
            {


                if (Canvas.GetTop(napadac) < 100)     //granica za vertikalno kretanje
                {
                    Canvas.SetLeft(napadac, Canvas.GetLeft(napadac) + brzinaNapadaca);

                    if (Canvas.GetLeft(napadac) > 820)     //provjera da li je napustio mapu
                    {
                        Canvas.SetLeft(napadac, -80);         //pocetna pozicija sa lijeve strane
                        Canvas.SetTop(napadac, Canvas.GetTop(napadac) + napadac.Height + 10);      //pomjeranje napadaca prema dole
                    }
                }
                else
                {
                    Canvas.SetLeft(napadac, -80);    //vracanje na pocetak horizontalno
                    Canvas.SetTop(napadac,20);             //vracanje na pocetak vertikalno
                }


                metakTimer1 -= 3;
                if (metakTimer1 < 0)    // simulacija pucanja vodje vanzemaljaca
                {
                    fabrikaMetakaNapadaca(Canvas.GetLeft(napadac)+napadac.Width/2, Canvas.GetTop(napadac)+napadac.Height);
                    metakTimer1 = metakTimerGranica;
                }

             

            }



            foreach (var x  in noviCanvas.Children.OfType<Rectangle>())
            {

                if(x is Rectangle && (string)x.Tag=="metak")
                {
                    Canvas.SetTop(x,Canvas.GetTop(x)-20);    //pomjeranje metka igraca prema gore
                    if (Canvas.GetTop(x) < 10)              //uklanjanje metka igraca ako se ne nalazi na ekranu
                        listaZaBrisanje.Add(x);               

                    Rect metakProstor = new Rect(Canvas.GetLeft(x),Canvas.GetTop(x),x.Width,x.Height);   //prostor na kome se metak nalazi

                    foreach (var y in noviCanvas.Children.OfType<Rectangle>())
                    {
                        if(y is Rectangle && (string)y.Tag == "napadac")
                        {
                            Rect pogodakProstor = new Rect(Canvas.GetLeft(y), Canvas.GetTop(y), y.Width, y.Height); 
                            if (metakProstor.IntersectsWith(pogodakProstor))  //provjera da li je metak pogodio napadaca
                            {
                                
                                listaZaBrisanje.Add(x);
                                listaZaBrisanje.Add(y);
                                brojNapadaca-=1;
                            }

                        }
                    }


                    if(garda && metakProstor.IntersectsWith(vanzemaljacProstor) )      //provjera da li je metak pogodio vodju vanzemaljaca
                    {
                        if (SnagaVanzemaljca < 1)
                        {

                           
                            listaZaBrisanje.Add(napadac);

                            brojNapadaca -= 1;
                           

                        }
                        healthBar.Width -= 20;
                        pogodjen = true;
                     
                         healthBar.Fill = new SolidColorBrush(Colors.Red);        

                        listaZaBrisanje.Add(x);
                        SnagaVanzemaljca--;
                    }

                }


                if(x is Rectangle && (string)x.Tag == "napadac")
                {
                    Canvas.SetLeft(x, Canvas.GetLeft(x)+brzinaNapadaca);     //pomjeranje napadaca na mapi u lijevo

                    if(Canvas.GetLeft(x) >820)     //provjera da li je napustio mapu
                    {
                        Canvas.SetLeft(x,-80);         //pocetna pozicija sa lijeve strane
                        Canvas.SetTop(x,Canvas.GetTop(x)+x.Height+10);      //pomjeranje napadaca prema dole
                    }

                    Rect napadacProstor = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);  //prostor dodjeljen napadacu
                    if(igracProstor.IntersectsWith(napadacProstor))  //sudar igraca i napadaca
                    {
                        prikazKrajaIgre("Ubijen si.");
                    }



                }
                if (x is Rectangle && (string)x.Tag == "napadackiMetak")
                {
                    Canvas.SetTop(x,Canvas.GetTop(x)+10);     //pomjeranje metka napadaca prema dole
                    if (Canvas.GetTop(x) > 480)         //ako je izasao sa mape sledi njegovo brisanje
                        listaZaBrisanje.Add(x);

                    Rect metakNapadacaProstor = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);

                    if (igracProstor.IntersectsWith(metakNapadacaProstor))   //igrac pogodjen
                    {
                        prikazKrajaIgre("Ubio te metak.");
                    }
                }    

            }

            foreach (Rectangle i in listaZaBrisanje)  //brisanje elemenata 
            {
                noviCanvas.Children.Remove(i);
            }

            if (brojNapadaca < 12)
                brzinaNapadaca = 12;     //ubrzanje napadaca
            if (brojNapadaca < 1 )               //provjera da li su ubijeni svi napadaci
            {

                if(garda==true)         // provjera da li je ubijen vodja vanzemaljaca
                    {
                    
                    prikazKrajaIgre("Pobjedio si,spasio si svijet!!!");
                    
                    
                }
               
              
                dodajVanzemaljca();     //dodavanje vodje vanzemaljaca
                garda = true;
               

            }

           
        }

        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            if(e.Key ==Key.Left)
                lijevo = true;
            if(e.Key ==Key.Right)
                desno = true;

        }

        private void KeyIsUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Left)
                lijevo= false;
            if(e.Key ==Key.Right)
                desno = false;
            if(e.Key==Key.Space)     //kreiranje metka igraca
            {
                Rectangle noviMetak = new Rectangle
                {
                    Tag = "metak",
                    Height = 20,
                    Width = 5,
                    Fill = Brushes.White,
                    Stroke = Brushes.Red
                };
                Canvas.SetLeft(noviMetak,Canvas.GetLeft(igrac)+igrac.Width/2);     //postavljanje kordinate metka horizontalno
                Canvas.SetTop(noviMetak, Canvas.GetTop(igrac)-noviMetak.Height);    //postavljanje kordinate metka vertikalno
                noviCanvas.Children.Add(noviMetak);
            }

            if (e.Key == Key.Enter && krajIgre == true)    //ponovno pokretanje igrice
            {
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
        }

       

        private void fabrikaMetakaNapadaca(double x,double y)   //postavljanje metka na odgovarajucu poziciju i njegovo kreiranje
        {
            Rectangle napadackiMetak = new Rectangle
            {

                Tag = "napadackiMetak",
                Height = 40,
                Width = 15,
                Fill = Brushes.Yellow,
                Stroke = Brushes.Black,
                StrokeThickness = 5

            };

            Canvas.SetTop(napadackiMetak, y);
            Canvas.SetLeft(napadackiMetak, x);
            noviCanvas.Children.Add(napadackiMetak);
        }

        private void fabrikaNapadaca(int granica)        //kreiranje napadaca
        {
            int pomjeri=800;
            brojNapadaca = granica;

            for(int i = 0;i<granica;i++)
            {
                ImageBrush napadacIzgled=new ImageBrush();
                Rectangle noviNapadac=new Rectangle
                {
                    Tag = "napadac",
                    Height = 45,
                    Width = 45,
                    Fill = napadacIzgled
                };

                Canvas.SetLeft(noviNapadac,pomjeri);   //pocetna horizontalna pozicija
                Canvas.SetTop(noviNapadac,30);         //pocetna vertikalna pozicija
                noviCanvas.Children.Add(noviNapadac);
                pomjeri -= 60;     //razlika horizontalne pozicije napadaca,svaki pomjeren u odnosu na drugog za 60 

                napadacSlike++;
                if (napadacSlike > 8)     //ako smo prosli kroz prvih 8 slika, idemo novi krug
                    napadacSlike = 1;

                switch (napadacSlike)   //dodavanje razlicitih slika za razlicite napadace
                {
                    case 1:
                        napadacIzgled.ImageSource = new BitmapImage(new Uri("pack://application:,,,/slike/invader1.gif"));
                        break;
                    case 2:
                        napadacIzgled.ImageSource = new BitmapImage(new Uri("pack://application:,,,/slike/invader2.gif"));
                        break;
                    case 3:
                        napadacIzgled.ImageSource = new BitmapImage(new Uri("pack://application:,,,/slike/invader3.gif"));
                        break;
                    case 4:
                        napadacIzgled.ImageSource = new BitmapImage(new Uri("pack://application:,,,/slike/invader4.gif"));
                        break;
                    case 5:
                        napadacIzgled.ImageSource = new BitmapImage(new Uri("pack://application:,,,/slike/invader5.gif"));
                        break;
                    case 6:
                        napadacIzgled.ImageSource = new BitmapImage(new Uri("pack://application:,,,/slike/invader6.gif"));
                        break;
                    case 7:
                        napadacIzgled.ImageSource = new BitmapImage(new Uri("pack://application:,,,/slike/invader7.gif"));
                        break;
                    case 8:
                        napadacIzgled.ImageSource = new BitmapImage(new Uri("pack://application:,,,/slike/invader8.gif"));
                        break;
                }
            }    
        }

        private void prikazKrajaIgre(string s)  //stopiranje igre
        {
            healthBar.Visibility = Visibility.Hidden;
            snaga.Visibility = Visibility.Hidden;
            krajIgre = true;
            timerIgre.Stop();
          
              
            
      
            brojNeprijatelja.Content = " " + s + " Pritisni Enter da nastavis sa igrom.";
        }
    }
}
