using System;
using System.Windows.Media;

namespace Echo_of_London___Morse_Defence
{
    public static class SoundHelper
    {
        private static MediaPlayer dzwiekKlikniecia;

        public static void PlayClick()
        {
            try
            {
                dzwiekKlikniecia = new MediaPlayer();
                dzwiekKlikniecia.Open(new Uri("Assets/Sounds/Click.mp3", UriKind.Relative));
                dzwiekKlikniecia.Volume = 0.5;
                dzwiekKlikniecia.Play();
            }
            catch { }
        }
    }
}