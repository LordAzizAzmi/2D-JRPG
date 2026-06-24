namespace WannaBHero
{
    public interface ISceneTransition
    {
        // onComplete = callback yang dipanggil SETELAH
        // animasi transisi selesai → scene baru di-load
        void PlayTransition(System.Action onComplete);
    }
}