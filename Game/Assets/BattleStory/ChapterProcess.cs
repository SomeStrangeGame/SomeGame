using System;
using BattleStory.SOData;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace BattleStory
{
    internal class ChapterProcess : BaseDisposable
    {
        internal struct Ctx
        {
            internal ChaptersData ChaptersData;

            internal ThreadPriority DefaultThreadPriority;

            internal Func<string, UniTask<string>> GetText;
            internal Func<string, string, UniTask<GameObject>> GetBundledPrefab;
            internal Func<string, string, UniTask<Sprite>> GetBundledSprite;
            internal Func<string, UniTask<AssetBundle>> GetAssetBundle;

            internal Func<UniTask> ShowLoading;
            internal Func<UniTask> HideLoading;
        }

        private Ctx _ctx;

        internal ChapterProcess(Ctx ctx)
        {
            _ctx = ctx;
        }

        internal async UniTask ShowChapterProcess(int index, bool skipIntro, bool skipStart, bool skipBattle, bool skipFailed, bool skipSuccess)
        {
            var storyProcessCtx = new StoryProcess.Ctx
            {
                DefaultThreadPriority = _ctx.DefaultThreadPriority,
                GetText = _ctx.GetText,
                GetBundledPrefab = _ctx.GetBundledPrefab,
                GetBundledSprite = _ctx.GetBundledSprite,
                GetAssetBundle = _ctx.GetAssetBundle,
                ShowLoading = _ctx.ShowLoading,
                HideLoading = _ctx.HideLoading,
            };

            var battleProcessCtx = new BattleProcess.Ctx
            {
                DefaultThreadPriority = _ctx.DefaultThreadPriority,
                GetBundledPrefab = _ctx.GetBundledPrefab,
                GetAssetBundle = _ctx.GetAssetBundle,
                ShowLoading = _ctx.ShowLoading,
                HideLoading = _ctx.HideLoading,
            };

            var chapterData = _ctx.ChaptersData.Chapters[index];
            
            if (!skipIntro)
            {
                for (var i = 0; i < chapterData.IntroMenu.Length; i++)
                {
                    var isLast = i + 1 >= chapterData.IntroMenu.Length;
                    if (!isLast)
                        using(var storyProcess = new StoryProcess(storyProcessCtx).AddTo(this))
                            await storyProcess.ShowMenuProcess(chapterData.IntroMenu[i], chapterData.IntroMenu[i + 1]);
                    else if (chapterData.StartMenu.Length > 0) 
                        using(var storyProcess = new StoryProcess(storyProcessCtx).AddTo(this))
                            await storyProcess.ShowMenuProcess(chapterData.IntroMenu[i], chapterData.StartMenu[0]);
                    else
                        using(var storyProcess = new StoryProcess(storyProcessCtx).AddTo(this))
                            await storyProcess.ShowMenuProcess(chapterData.IntroMenu[i]);
                }
            }

            if (!skipStart)
            {
                for (var i = 0 ; i < chapterData.StartMenu.Length; i++)
                {
                    var isLast = i + 1 >= chapterData.StartMenu.Length;
                    if (!isLast)
                        using(var storyProcess = new StoryProcess(storyProcessCtx).AddTo(this))
                            await storyProcess.ShowMenuProcess(chapterData.StartMenu[i], chapterData.StartMenu[i + 1]);
                    else if (chapterData.Battles.Length > 0)
                        using(var storyProcess = new StoryProcess(storyProcessCtx).AddTo(this))
                            await storyProcess.ShowMenuProcess(chapterData.StartMenu[i], chapterData.Battles[0]);
                    else
                        using(var storyProcess = new StoryProcess(storyProcessCtx).AddTo(this))
                            await storyProcess.ShowMenuProcess(chapterData.StartMenu[i]);
                }
            }

            var battleResult = 1;
            if (!skipBattle)
            {
                for (var i = 0 ; i < chapterData.Battles.Length; i++)
                {
                    var isLast = i + 1 >= chapterData.Battles.Length;
                    if (!isLast)
                        using(var battleProcess = new BattleProcess(battleProcessCtx).AddTo(this))
                            battleResult = await battleProcess.ShowBattleProcess(chapterData.Battles[i], chapterData.Battles[i + 1]);
                    else if (chapterData.FailedMenu.Length > 0 && chapterData.SuccessMenu.Length > 0)
                        using(var battleProcess = new BattleProcess(battleProcessCtx).AddTo(this))
                            battleResult = await battleProcess.ShowBattleProcess(chapterData.Battles[i], chapterData.FailedMenu[0], chapterData.SuccessMenu[0]);
                    else
                        using(var battleProcess = new BattleProcess(battleProcessCtx).AddTo(this))
                            battleResult = await battleProcess.ShowBattleProcess(chapterData.Battles[i]);
                    if (battleResult == 0)
                        break;
                }
            }

            if (battleResult == 0) //failed
            {
                if (!skipFailed)
                {
                    for (var i = 0 ; i < chapterData.FailedMenu.Length; i++)
                    {
                        var isLast = i + 1 >= chapterData.FailedMenu.Length;
                        if (!isLast)
                            using(var storyProcess = new StoryProcess(storyProcessCtx).AddTo(this))
                                await storyProcess.ShowMenuProcess(chapterData.FailedMenu[i], chapterData.FailedMenu[i + 1]);
                        else
                            using(var storyProcess = new StoryProcess(storyProcessCtx).AddTo(this))
                                await storyProcess.ShowMenuProcess(chapterData.FailedMenu[i]);
                    }
                }

                ShowChapterProcess(index, true, false, false, false, false).Forget();
                return;
            }

            if (!skipSuccess)
            {
                for (var i = 0; i < chapterData.SuccessMenu.Length; i++)
                {
                    var isLast = i + 1 >= chapterData.SuccessMenu.Length;
                    if (!isLast)
                        using(var storyProcess = new StoryProcess(storyProcessCtx).AddTo(this))
                            await storyProcess.ShowMenuProcess(chapterData.SuccessMenu[i], chapterData.SuccessMenu[i + 1]);
                    else
                        using(var storyProcess = new StoryProcess(storyProcessCtx).AddTo(this))
                            await storyProcess.ShowMenuProcess(chapterData.SuccessMenu[i]);
                }
            }

            index++;
            if (index < _ctx.ChaptersData.Chapters.Length)
            {
                ShowChapterProcess(index, false, false, false, false, false).Forget();
                return;
            }

            ShowChapterProcess(0, false, false, false, false, false).Forget(); //restart
        }
    }
}

