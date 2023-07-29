using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace FireInTheHole.Audio;

public class SfxController 
{
    public readonly GameEngine _engine;

    public SfxController(GameEngine engine)
    {
        _engine = engine;

        LoadContent();
    }

    public SoundEffect Explosion { get; private set; }

    public SoundEffect Fuse { get; private set; }

    public Song Music { get; private set; }

    private void LoadContent()
    {
        Explosion = _engine.Content.Load<SoundEffect>("sfx_explosion");
        Fuse = _engine.Content.Load<SoundEffect>("sfx_fuse");
        Music = _engine.Content.Load<Song>("music");
    }

    public void PlayMusic()
    {
        MediaPlayer.Volume = 0.2f;
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Play(Music);
    }

    public void PlayExplosion()
    {
        Explosion.Play();
    }

    public void PlayFuse()
    {
        Fuse.Play();
    }
}