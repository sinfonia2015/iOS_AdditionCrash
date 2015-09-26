﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Fresvii.AppSteroid.UI
{
    public class AUILeaderboardScrollView : MonoBehaviour
    {
        public enum Mode { Daily, Weekly, Whole };

        public AUILeaderboard auiLeaderboard;

        public GameObject prfbPlayerScoreCell;

        public AUIScrollViewContents contents;

        public Mode mode;

        private List<AUIPlayerScoreCell> cells = new List<AUIPlayerScoreCell>();

        public AUIPlayerScoreCell myScoreCell;

        private Fresvii.AppSteroid.Models.ListMeta meta;

        private bool isPullRefleshProc;

        public AUIScrollViewPullReflesh pullReflesh;

        public AUIScrollRect scrollView;

        private Fresvii.AppSteroid.Models.ListMeta listMeta;

        private FASLeaderboard.Period period;

        public AUIScrollViewItemLayout[] myCellItems;

        public AUIScrollViewItemLayout[] allPlayerCellItems;

        public GameObject noData;

        uint? maxPage = 1;

        public AUIVerticalLayoutPullReflesh noDataPullReflesh;

        void Awake()
        {
            period = (FASLeaderboard.Period)System.Enum.Parse(typeof(FASLeaderboard.Period), mode.ToString(), true);

            foreach (AUIScrollViewItemLayout item in myCellItems)
            {
                item.gameObject.SetActive(false);
            }
        }

        void OnEnable()
        {
            pullReflesh.OnPullDownReflesh += OnPullDownReflesh;

            pullReflesh.OnPullUpReflesh += OnPullUpReflesh;

            noDataPullReflesh.OnPullDownReflesh += OnVPullDownReflesh;

            if (listMeta != null)
            {
                noData.SetActive(listMeta.TotalCount == 0);
            }
            else
            {
                noData.SetActive(false);
            }

            StartCoroutine(Init());
        }

        void OnDisable()
        {
            pullReflesh.OnPullDownReflesh -= OnPullDownReflesh;

            pullReflesh.OnPullUpReflesh -= OnPullUpReflesh;

            noDataPullReflesh.OnPullDownReflesh -= OnVPullDownReflesh;
        }

        void OnPullDownReflesh()
        {
            isPullRefleshProc = true;

            GetInitRanking();        
        }

        bool isVpullDownReflesh;

        void OnVPullDownReflesh()
        {
            isPullRefleshProc = true;

            isVpullDownReflesh = true;

            GetInitRanking();
        }

        void OnPullUpReflesh()
        {
            if (listMeta != null && listMeta.TotalPages >= maxPage && !isPullRefleshProc)
            {
                isPullRefleshProc = true;

                maxPage++;

                FASLeaderboard.GetRanking(auiLeaderboard.Leaderboard.Id, period, false, (uint)maxPage, OnGetRanking);           
            }
            else
            {
                pullReflesh.PullRefleshCompleted();
            }
        }

        IEnumerator Init()
        {
            while (!AUIManager.Instance.Initialized)
            {
                yield return 1;
            }

            while (auiLeaderboard.Leaderboard == null)
            {
                yield return 1;
            }

            GetInitRanking();        
        }

        void GetInitRanking()
        {
            FASLeaderboard.GetUserRank(auiLeaderboard.Leaderboard.Id, FAS.CurrentUser.Id, period, (rank, error) =>
            {
                if (error == null)
                {
                    if (rank != null)
                    {
                        rank.Score.User = FAS.CurrentUser;

                        myScoreCell.SetScore(rank.Score, (user) =>
                        {
                            auiLeaderboard.GoToUserPage(user);
                        });

                        myScoreCell.Rank = rank.Ranking;

                        foreach (AUIScrollViewItemLayout item in myCellItems)
                        {
                            item.gameObject.SetActive(true);

                            item.ignore = false;
                        }

                        contents.ReLayout();
                    }
                }
            });

            FASLeaderboard.GetRanking(auiLeaderboard.Leaderboard.Id, period, false, 1, OnGetRanking);           

        }

        void OnGetRanking(IList<Fresvii.AppSteroid.Models.Score> scores, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
        {
            if (this == null || !this.gameObject.activeInHierarchy)
            {
                return;
            }

            if (error != null)
            {
                if (FASSettings.Instance.logLevel <= FAS.LogLevels.Error)
                {
                    Debug.LogError(error.ToString());
                }

                return;
            }

            if (isVpullDownReflesh)
            {
                isVpullDownReflesh = false;

                isPullRefleshProc = false;

                noDataPullReflesh.PullRefleshCompleted();
            }
            else if (isPullRefleshProc)
            {
                pullReflesh.PullRefleshCompleted();

                isPullRefleshProc = false;
            }

            auiLeaderboard.StartCoroutine(SetRanking(scores, meta, error));
        }

        IEnumerator SetRanking(IList<Fresvii.AppSteroid.Models.Score> scores, Fresvii.AppSteroid.Models.ListMeta meta, Fresvii.AppSteroid.Models.Error error)
        {            
            while (this != null && !this.gameObject.activeInHierarchy)
            {
                yield return 1;
            }

            if (this == null || !this.gameObject.activeInHierarchy)
            {
                yield break;
            }

            foreach (AUIScrollViewItemLayout item in allPlayerCellItems)
            {
                item.ignore = false;
            }

            this.listMeta = meta;

            noData.SetActive(meta.TotalCount == 0);

            foreach (Fresvii.AppSteroid.Models.Score score in scores)
            {
                UpdateScore(score);
            }

            Sort();
        }

        private void Sort()
        {
            // Sort
            if (auiLeaderboard.Leaderboard.Ascend)
            {
                cells.Sort((a, b) => CompareScore(b, a));
            }
            else
            {
                cells.Sort((a, b) => CompareScore(a, b));
            }

            foreach (AUIPlayerScoreCell obj in cells)
            {
                obj.transform.SetSiblingIndex(contents.transform.childCount - 1);
            }

            int preScoreValue = (auiLeaderboard.Leaderboard.Ascend) ? int.MaxValue : int.MinValue;

            for (int i = 0; i < cells.Count; i++)
            {
                if (preScoreValue == cells[i].Score.Value)
                {
                    if (i != 0)
                    {
                        cells[i].Rank = cells[i - 1].Rank;
                    }
                    else
                    {
                        cells[i].Rank = i + 1;
                    }
                }
                else
                {
                    cells[i].Rank = i + 1;
                }

                preScoreValue = cells[i].Score.Value;
            }

            contents.ReLayout();
        }

        int CompareScore(AUIPlayerScoreCell x, AUIPlayerScoreCell y)
        {
            if (y.Score.Value == x.Score.Value)
            {
                return System.DateTime.Compare(y.Score.CreatedAt, x.Score.CreatedAt);
            }
            else
            {
                return y.Score.Value - x.Score.Value;
            }
        }

        private bool UpdateScore(Fresvii.AppSteroid.Models.Score score)
        {
            AUIPlayerScoreCell cell = cells.Find(x => x.Score.Id == score.Id);

            if (cell != null)
            {
                cell.SetScore(score, (user) =>
                {
                    auiLeaderboard.GoToUserPage(user);
                });

                return false;
            }

            GameObject go = Instantiate(prfbPlayerScoreCell) as GameObject;

            var item = go.GetComponent<RectTransform>();

            contents.AddItem(item);

            cell = item.GetComponent<AUIPlayerScoreCell>();

            cell.SetScore(score, (user) =>
            {
                auiLeaderboard.GoToUserPage(user);
            });

            cells.Add(cell);

            cell.gameObject.SetActive(false);

            return true;
        }        
    }
}
