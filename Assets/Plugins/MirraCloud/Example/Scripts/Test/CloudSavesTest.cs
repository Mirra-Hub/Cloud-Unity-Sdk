using System.Collections.Generic;
using MirraCloud.Core;
using MirraCloud.Core.CloudSave;
using MirraCloud.Core.CloudSave.Requests;
using UnityEngine;

namespace Plugins.MirraCloud.Example.Scripts.Test
{
    public enum RegionValue
    {
        RU,
        US,
        EU
    }
    
    public class CloudSavesTest : MonoBehaviour
    {
        [SerializeField] private int _numberValue = 10;
        [SerializeField] private int _factorValue = 2;
        [SerializeField] private RegionValue _regionValue;
        [SerializeField] private CloudSaveIndexOp _filterOp = CloudSaveIndexOp.Equal;
        [SerializeField] private int _filterValue = 10;
        [SerializeField] [TextArea(100, 200)] private string _searchResult;
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                Save();
            }
            
            if (Input.GetKeyDown(KeyCode.L))
            {
                Load();
            }
            
            if (Input.GetKeyDown(KeyCode.P))
            {
                SearchPlayers();
            }
        }

    

        public void Save()
        {
     
            var cloudSaveData = new CloudSaveDataRequest();
            cloudSaveData.AddInt("level", _numberValue);
            cloudSaveData.AddInt("attack", _factorValue);
            cloudSaveData.AddString("region", ConvertRegion(_regionValue));
            cloudSaveData.WithDefaultAccess(AccessMask.Other, AccessMask.Owner);
            
            var op = MirraCloudSDK.CloudSave.SaveAsync(cloudSaveData);
         
        }
        
        public async void Load()
        {
            var op = MirraCloudSDK.CloudSave.LoadAsync();
            await op.Task();

            foreach (var response in op.Result.Data)
            {
                Debug.Log($"Cloud Code result: {response.key}, {response.value}");
            }
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
            
            var op = MirraCloudSDK.CloudSave.QueryPlayerDataAsync(queryRequest);
            await  op.Task();

            _searchResult = op.Result.ResponseBody;
        }
    }
}