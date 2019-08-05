﻿using IoTManager.Core.Infrastructures;
using IoTManager.IDao;
using IoTManager.IHub;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using IoTManager.Model;
using IoTManager.Utility.Serializers;
using Microsoft.Azure.Devices;

namespace IoTManager.Core
{
    public sealed class DeviceBus:IDeviceBus
    {
        private readonly IDeviceDao _deviceDao;
        private readonly IoTHub _iotHub;
        private readonly ILogger _logger;
        public DeviceBus(IDeviceDao deviceDao,IoTHub iotHub,ILogger<DeviceBus> logger)
        {
            this._deviceDao = deviceDao;
            this._iotHub = iotHub;
            this._logger = logger;
        }

        public List<DeviceSerializer> GetAllDevices(int page)
        {
            int offset = (page - 1) * 12;
            int limit = 12;
            List<DeviceModel> devices = this._deviceDao.Get(offset, limit);
            List<DeviceSerializer> result = new List<DeviceSerializer>();
            foreach (DeviceModel device in devices)
            {
                result.Add(new DeviceSerializer(device));
            }
            return result;
        }

        public DeviceSerializer GetDeviceById(int id)
        {
            DeviceModel device = this._deviceDao.GetById(id);
            DeviceSerializer result = new DeviceSerializer(device);
            return result;
        }

        public List<DeviceSerializer> GetDevicesByDeviceName(String deviceName)
        {
            List<DeviceModel> devices = this._deviceDao.GetByDeviceName(deviceName);
            List<DeviceSerializer> result = new List<DeviceSerializer>();
            foreach (DeviceModel device in devices)
            {
                result.Add(new DeviceSerializer(device));
            }
            return result;
        }

        public List<DeviceSerializer> GetDevicesByDeviceId(String deviceId)
        {
            List<DeviceModel> devices = this._deviceDao.GetByDeviceId(deviceId);
            List<DeviceSerializer> result = new List<DeviceSerializer>();
            foreach (DeviceModel device in devices)
            {
                result.Add(new DeviceSerializer(device));
            }
            return result;
        }

        public String CreateNewDevice(DeviceSerializer deviceSerializer)
        {
            DeviceModel deviceModel = new DeviceModel();
            deviceModel.HardwareDeviceId = deviceSerializer.hardwareDeviceID;
            deviceModel.DeviceName = deviceSerializer.deviceName;
            deviceModel.City = deviceSerializer.city;
            deviceModel.Factory = deviceSerializer.factory;
            deviceModel.Workshop = deviceSerializer.workshop;
            deviceModel.DeviceState = deviceSerializer.deviceState;
            deviceModel.ImageUrl = deviceSerializer.imageUrl;
            deviceModel.GatewayId = deviceSerializer.gatewayId;
            deviceModel.Mac = deviceSerializer.mac;
            deviceModel.DeviceType = deviceSerializer.deviceType;
            deviceModel.Remark = deviceSerializer.remark;
            return this._deviceDao.Create(deviceModel);
        }

        public String UpdateDevice(int id, DeviceSerializer deviceSerializer)
        {
            DeviceModel deviceModel = new DeviceModel();
            deviceModel.Id = id;
            deviceModel.HardwareDeviceId = deviceSerializer.hardwareDeviceID;
            deviceModel.DeviceName = deviceSerializer.deviceName;
            deviceModel.City = deviceSerializer.city;
            deviceModel.Factory = deviceSerializer.factory;
            deviceModel.Workshop = deviceSerializer.workshop;
            deviceModel.DeviceState = deviceSerializer.deviceState;
            deviceModel.ImageUrl = deviceSerializer.imageUrl;
            deviceModel.GatewayId = deviceSerializer.gatewayId;
            deviceModel.Mac = deviceSerializer.mac;
            deviceModel.DeviceType = deviceSerializer.deviceType;
            deviceModel.Remark = deviceSerializer.remark;
            return this._deviceDao.Update(id, deviceModel);
        }

        public String DeleteDevice(int id)
        {
            return this._deviceDao.Delete(id);
        }

        public int BatchDeleteDevice(int[] id)
        {
            return this._deviceDao.BatchDelete(id);
        }

        public List<DeviceSerializer> GetDeviceByWorkshop(String city, String factory, String workshop)
        {
            List<DeviceModel> devices = _deviceDao.GetByWorkshop(city, factory, workshop);
            List<DeviceSerializer> result = new List<DeviceSerializer>();
            foreach (DeviceModel d in devices)
            {
                result.Add(new DeviceSerializer(d));
            }

            return result;
        }

        public int GetDeviceAmount()
        {
            return this._deviceDao.GetDeviceAmount();
        }

        public List<object> GetDeviceTree(String city, String factory)
        {
            return this._deviceDao.GetDeviceTree(city, factory);
        }

        public String CreateDeviceType(String deviceType)
        {
            return this._deviceDao.CreateDeviceType(deviceType);
        }
    }
}
