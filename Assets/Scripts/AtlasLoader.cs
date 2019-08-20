using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class SpriteData
{
    public string Name;
    public int index;
    public Sprite sprite;
}

/// <summary>
/// Attach multi sprite resourse to provide fast connection to the Sprite sheet.
/// </summary>
[Serializable]
public class AtlasLoader : MonoBehaviour
{
    private const string AlreadySelectedAtlasMsg = "{0} (AtlasLoader): Atlas is already selected.";
    private const string SuccesfullyLoadedMsg = "{0} (AtlasLoader): Succesfully fill atlas.";
    private const string AtlasNotSelectedMsg = "{0} (AtlasLoader): Atlas not selected.";

    public static AtlasLoader instance;

    /// <summary>
    /// When is ready with initilize the atlas lock the validation of the sprite array.  Click that to true for better editor performance.
    /// </summary>
    public bool lockAtlas;

    /// <summary>
    /// Attach sprite sheet which provide the sprites.
    /// </summary>
    [SerializeField]
    public Sprite Atlas;

    /// <summary>
    /// Array of Sprites from the atlas.
    /// </summary>
    [SerializeField]
    public SpriteData[] Sprites;

    private string selectedAtlasPath;

    private Dictionary<string, SpriteData> spriteRepo;
    private string selectedAtlasName;

    private Sprite FillRepoUntilGetOrNull(string spriteName)
    {
        if (this.spriteRepo == null)
        {
            this.spriteRepo = new Dictionary<string, SpriteData>();
        }

        for (int i = 0; i < this.Sprites.Length; i++)
        {
            if (!this.spriteRepo.ContainsKey(this.Sprites[i].Name))
            {
                this.spriteRepo.Add(this.Sprites[i].Name, this.Sprites[i]);
            }
            else
            {
                this.spriteRepo[this.Sprites[i].Name] = this.Sprites[i];
            }

            if (spriteName == this.Sprites[i].Name)
            {
                return this.Sprites[i].sprite;
            }
        }

        return null;
    }

    /// <summary>
    /// Get sprite from sprites repository by sprite name.<para></para>
    /// Note that when the repository is first use fill repo and get first accurance.<para></para>
    /// On second search when don,t have that key continue filling the repository while sprite repo length is equals to Sprites array count.
    /// </summary>
    /// <param name="spriteName">Name of the sprite you looking for.</param>
    /// <returns>Sprite which looking for or null when doesnt found.</returns>
    public Sprite Get(string spriteName)
    {
        if (this.spriteRepo == null)
        {
            return FillRepoUntilGetOrNull(spriteName);
        }

        if (this.spriteRepo.ContainsKey(spriteName))
        {
            return this.spriteRepo[spriteName].sprite;
        }

        if (this.spriteRepo.Count != this.Sprites.Length)
        {
            return FillRepoUntilGetOrNull(spriteName);
        }

        return null;
    }

    /// <summary>
    /// Get Sprite and returns it by index from Sprite array.
    /// </summary>
    /// <param name="spriteIndex">Index of the sprite</param>
    /// <returns>Sprite of that index.</returns>
    public Sprite Get(int spriteIndex)
    {
        return this.Sprites[spriteIndex].sprite;
    }

#if UNITY_EDITOR
    private void FillAtlas()
    {
        if (this.Atlas == null)
        {
            Debug.LogWarning(string.Format(AtlasNotSelectedMsg, this.name));
            this.selectedAtlasPath = null;
            this.selectedAtlasName = null;
            return;
        }

        if (this.Atlas.name == this.selectedAtlasName)
        {
            Debug.LogWarning(string.Format(AtlasNotSelectedMsg, this.name));
            return;
        }

        this.selectedAtlasName = this.Atlas.name;
        string path = AssetDatabase.GetAssetPath(this.Atlas);

        if (this.selectedAtlasPath == path)
        {
            Debug.Log(string.Format(AlreadySelectedAtlasMsg, this.name));
            return;
        }

        this.selectedAtlasPath = path;
        Object[] spritesArr = AssetDatabase.LoadAllAssetsAtPath(path);
        this.Sprites = new SpriteData[spritesArr.Length - 1];

        for (int i = 1; i < spritesArr.Length; i++)
        {
            Sprite sprite = spritesArr[i] as Sprite;
            if (sprite != null)
            {
                this.Sprites[i - 1] = new SpriteData { index = i - 1, Name = sprite.name, sprite = sprite };
            }
        }

        Debug.Log(string.Format(SuccesfullyLoadedMsg, this.name));
    }

    public void OnValidate()
    {
        if (!Application.isPlaying && !this.lockAtlas)
        {
            FillAtlas();
        }
    }
#endif
}