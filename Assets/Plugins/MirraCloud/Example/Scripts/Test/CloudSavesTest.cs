using System.Collections.Generic;
using MirraCloud.Core;
using MirraCloud.Core.CloudSave;
using MirraCloud.Core.CloudSave.Requests;
using MirraCloud.Example.Infrastructure.DI;
using NaughtyAttributes;
using UnityEngine;

namespace Plugins.MirraCloud.Example.Scripts.Test
{
    public class CloudSavesTest : MonoBehaviour
    {
        [SerializeField] private int _numberValue = 10;
        [SerializeField] private int _factorValue = 2;
        [SerializeField] private RegionValue _regionValue;
        [SerializeField] private CloudSaveIndexOp _filterOp = CloudSaveIndexOp.Equal;
        [SerializeField] private int _filterValue = 10;
        [SerializeField] private string _customId;

        private IMirraCloudSdk _sdk;

        private enum RegionValue
        {
            RU,
            US,
            EU
        }

        [InjectDep]
        public void Construct(IMirraCloudSdk sdk)
        {
            _sdk = sdk;
        }

        [Button]
        private void Save()
        {
            var cloudSaveData = new CloudSaveDataRequest();
            cloudSaveData.AddInt("level", _numberValue);
            cloudSaveData.AddInt("attack", _factorValue);
            cloudSaveData.AddString("region", ConvertRegion(_regionValue));
            cloudSaveData.WithDefaultAccess(AccessMask.Other, AccessMask.Owner);

            var op = _sdk.CloudSave.SaveAsync(cloudSaveData);
        }

        [Button]
        public async void Load()
        {
            var op = _sdk.CloudSave.LoadAsync();
            await op.Task();

            _searchResult = op.Result.ResponseBody;
        }

        private string ConvertRegion(RegionValue region)
        {
            switch (region)
            {
                case RegionValue.RU:
                    return "ru";
                case RegionValue.US:
                    return "us";
            }

            return "eu";
        }

        [Button]
        private async void SearchPlayers()
        {
            var queryRequest = new QueryIndexRequest();
            queryRequest.limit = 10;
            queryRequest.returnKeys = new[]
            {
                "level",
                "region",
            };
            queryRequest.filters = new List<QueryFilter>()
            {
                new QueryFilter()
                {
                    key = "level",
                    op = _filterOp,
                    value = _filterValue
                }
            };

            var op = _sdk.CloudSave.QueryPlayerDataAsync(queryRequest);
            await op.Task();

            _searchResult = op.Result.ResponseBody;
        }

        [Button]
        private async void SaveGlobal()
        {
            var cloudSaveData = new CloudSaveDataRequest();
            cloudSaveData.AddInt("level_global", _numberValue);
            cloudSaveData.AddInt("global_counter", _numberValue);
            cloudSaveData.AddString("region", ConvertRegion(_regionValue));
            cloudSaveData.WithDefaultAccess(AccessMask.Other, AccessMask.Owner);

            var op = _sdk.CloudSave.SaveGlobalDataAsync(cloudSaveData);
            await op.Task();

            _searchResult = op.Result.ResponseBody;
        }

        [Button]
        private async void LoadGlobal()
        {
            var op = _sdk.CloudSave.LoadGlobalDataAsync(new[]
            {
                "level_global",
                "global_counter",
            });
            await op.Task();

            _searchResult = op.Result.ResponseBody;
        }

        [Button]
        private async void SaveCustom()
        {
            var cloudSaveData = new CloudSaveDataRequest();
            cloudSaveData.AddInt("level_global", _numberValue);
            cloudSaveData.AddInt("global_counter", _numberValue);
            cloudSaveData.WithDefaultAccess(AccessMask.Other, AccessMask.Owner);

            var op = _sdk.CloudSave.SaveCustomDataAsync(_customId, cloudSaveData);
            await op.Task();

            _searchResult = op.Result.ResponseBody;
        }

        [Button]
        private async void LoadCustom()
        {
            var op = _sdk.CloudSave.LoadCustomDataAsync(_customId, new[]
            {
                "level_global",
                "global_counter",
            });
            await op.Task();

            _searchResult = op.Result.ResponseBody;
        }

        [SerializeField] [TextArea(minLines: 20, maxLines: 20)]
        private string _searchResult;
    }
}