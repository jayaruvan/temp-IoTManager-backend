using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using IoTManager.Core.Infrastructures;
using IoTManager.IDao;
using IoTManager.Model;
using IoTManager.Utility.Serializers;
using Microsoft.Extensions.Logging;

namespace IoTManager.Core
{
    public class StateTypeBus: IStateTypeBus
    {
        private readonly IStateTypeDao _stateTypeDao;
        private readonly ILogger _logger;

        public StateTypeBus(IStateTypeDao stateTypeDao, ILogger<StateTypeBus> logger)
        {
            this._stateTypeDao = stateTypeDao;
            this._logger = logger;
        }
        
        public List<object> GetAllDeviceTypes()
        {
            List<String> deviceTypes = this._stateTypeDao.GetDeviceType();
            List<object> result = new List<object>();
            foreach (String dt in deviceTypes)
            {
                result.Add(new {deviceTypeName = dt});
            }
            return result;
        }

        public List<object> GetAllDeviceStates()
        {
            List<String> deviceStates = this._stateTypeDao.GetDeviceState();
            List<object> result = new List<object>();
            foreach (String ds in deviceStates)
            {
                result.Add(new {stateName = ds});   
            }
            return result;
        }

        public List<object> GetAllGatewayTypes()
        {
            List<String> gatewayTypes = this._stateTypeDao.GetGatewayType();
            List<object> result = new List<object>();
            foreach (String gt in gatewayTypes)
            {
                result.Add(new {gatewayTypeName = gt});
            }
            return result;
        }

        public List<object> GetAllGatewayStates()
        {
            List<String> gatewayStates = this._stateTypeDao.GetGatewayState();
            List<object> result = new List<object>();
            foreach (String gs in gatewayStates)
            {
                result.Add(new {stateName = gs});
            }
            return result;
        }

        public List<DeviceTypeSerializer> GetDetailedDeviceTypes(int pageMode = 0, int page = 1, String sortColumn = "id", String order = "asc")
        {
            int offset = (page - 1) * 12;
            int limit = 12;
            List<DeviceTypeModel> deviceTypes = this._stateTypeDao.GetDetailedDeviceType(pageMode, offset, limit, sortColumn, order);
            List<DeviceTypeSerializer> result = new List<DeviceTypeSerializer>();
            foreach (DeviceTypeModel dtm in deviceTypes)
            {
                result.Add(new DeviceTypeSerializer(dtm));
            }

            return result;
        }

        public String AddDeviceType(DeviceTypeSerializer deviceTypeSerializer)
        {
            DeviceTypeModel deviceTypeModel = new DeviceTypeModel();
            deviceTypeModel.Id = deviceTypeSerializer.id;
            deviceTypeModel.DeviceTypeName = deviceTypeSerializer.deviceTypeName;
            deviceTypeModel.OfflineTime = deviceTypeSerializer.offlineTime;
            return this._stateTypeDao.AddDeviceType(deviceTypeModel);
        }

        public String UpdateDeviceType(int id, DeviceTypeSerializer deviceTypeSerializer)
        {
            DeviceTypeModel deviceTypeModel = new DeviceTypeModel();
            deviceTypeModel.Id = deviceTypeSerializer.id;
            deviceTypeModel.DeviceTypeName = deviceTypeSerializer.deviceTypeName;
            deviceTypeModel.OfflineTime = deviceTypeSerializer.offlineTime;
            return this._stateTypeDao.UpdateDeviceType(id, deviceTypeModel);
        }

        public String DeleteDeviceType(int id)
        {
            return this._stateTypeDao.DeleteDeviceType(id);
        }

        public int GetDeviceTypeAffiliateDevice(int id)
        {
            return this._stateTypeDao.GetDeviceTypeAffiliateDevice(id);
        }

        public long GetDetailedDeviceTypeNumber()
        {
            return this._stateTypeDao.GetDetailedDeviceTypeNumber();
        }

        public DeviceTypeSerializer GetDeviceTypeByName(String name)
        {
            DeviceTypeModel tmp = this._stateTypeDao.GetDeviceTypeByName(name);
            DeviceTypeSerializer result = new DeviceTypeSerializer(tmp);
            return result;
        }
    }
}