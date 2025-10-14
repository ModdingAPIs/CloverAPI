using CloverAPI.Content.Audio;
using CloverAPI.Content.Textures;
using CloverAPI.Utils;

namespace CloverAPI.Patches;

[HarmonyPatch]
public class TexturePatcher
{
    [HarmonyPatch(typeof(Object), nameof(Object.Internal_CloneSingle), typeof(Object))]
    [HarmonyPostfix]
    public static void Object_CloneSingle(ref Object __result)
    {
        if (__result is GameObject gameObject)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (!material.HasProperty("_MainTex") || material.mainTexture == null)
                    {
                        continue;
                    }
                    string shortTexName = material.mainTexture.name.ToLower();
                    string longTexName = gameObject.name.Replace("(Clone)", "").Trim().ToLower() + "_" +
                                                              material.mainTexture.name.ToLower();
                    string longRendTexName = gameObject.name.Replace("(Clone)", "").Trim().ToLower() + "_" +
                                            renderer.name.Trim().ToLower() + "_" +
                                          material.mainTexture.name.ToLower();

                    if (TextureManager.TryGetTextureOverride(out var tex, shortTexName, longTexName, longRendTexName))
                    {
                        if ((material.mainTexture.width != tex.width ||
                            material.mainTexture.height != tex.height) &&
                             !Plugin.UseFullQualityTextures.Value)
                        {
                            tex = TextureUtils.ResizeTexture(tex, material.mainTexture.width, material.mainTexture.height);
                        }

                        material.mainTexture = tex;
                    }
                }
            }

            AudioSource[] audioSources = gameObject.GetComponentsInChildren<AudioSource>(true);
            foreach (AudioSource audioSource in audioSources)
            {
                if (audioSource.clip == null)
                {
                    continue;
                }

                if (AudioManager.TryGetSoundOverride(out var clip, audioSource.clip.name))
                {
                    audioSource.clip = clip;
                }
            }
        }
    }
}