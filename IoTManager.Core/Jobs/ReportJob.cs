using System;
using System.Collections.Generic;
using System.Linq;
using IoTManager.IDao;
using IoTManager.Model;
using Microsoft.Extensions.Logging;
using Pomelo.AspNetCore.TimedJob;

namespace IoTManager.Core.Jobs
{
    public class ReportJob: Job
    {
        private readonly IDeviceDataDao _deviceDataDao;
        private readonly IDeviceDao _deviceDao;
        private readonly IDeviceDailyOnlineTimeDao _deviceDailyOnlineTimeDao;
        private readonly ILogger _logger;

        public ReportJob(IDeviceDataDao deviceDataDao,
            IDeviceDao deviceDao,
            IDeviceDailyOnlineTimeDao deviceDailyOnlineTimeDao,
            ILogger<ReportJob> logger)
        {
            this._deviceDataDao = deviceDataDao;
            this._deviceDao = deviceDao;
            this._deviceDailyOnlineTimeDao = deviceDailyOnlineTimeDao;
            this._logger = logger;
        }
        /* ��ʱ������ͳ��ÿ���豸ÿ�������ʱ��
         * ���⣺����豸һ��ֻ�ϱ�һ�����ݣ����ж�����ʱ��Ϊ0
         */
        [Invoke(Begin = "2019-6-16 23:58", Interval = 1000*3600*24,SkipWhileExecuting = true)]
        public void Run()
        {
            _logger.LogInformation("ͳ�������豸ÿ������ʱ��...");

            List<DeviceModel> devices = _deviceDao.Get("all");
            int deviceNumber = devices.Count();
            foreach (var d in devices)
            {
                //_logger.LogInformation("ʣ�� " + (deviceNumber - 1).ToString() + " �豸");
                _logger.LogInformation("�豸����" + d.DeviceName);
                //��ȡ�豸�����һ������
                DeviceDataModel deviceData = _deviceDataDao.ListByDeviceNameASC(d.DeviceName, 1).FirstOrDefault();
                string text = deviceData != null ? d.DeviceName + "�豸����ʱ�䣺" + deviceData.Timestamp.ToString()
                    : d.DeviceName + "�豸û������";
                _logger.LogInformation(text);
                //��������
                //��ֹʱ��ĳ�00:00-23:59
                DateTime nowadays = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                double totalDays = deviceData != null ? (nowadays - new DateTime(deviceData.Timestamp.Year,deviceData.Timestamp.Month, deviceData.Timestamp.Day)).TotalDays
                    : 0;
                string text1 = deviceData != null ? ("������豸����ʱ��: " + deviceData.Timestamp + " ���������� " + totalDays + " ��")
                    : "û���豸����";
                _logger.LogInformation(text1);
                for (double i = 0; i < totalDays; i++)
                {
                    DateTime sTime = new DateTime(deviceData.Timestamp.Year, deviceData.Timestamp.Month, deviceData.Timestamp.Day);
                    _logger.LogInformation(sTime.AddDays(i).ToString());
                    //List<DeviceDataModel> deviceDatas = _deviceDataDao.ListByNameTimeASC(d.DeviceName, sTime.AddDays(i), sTime.AddDays(i+1), 20000);
                    DeviceDataModel firstdeviceData = _deviceDataDao.ListByNameTimeASC(d.DeviceName, sTime.AddDays(i), sTime.AddDays(i + 1), 1).FirstOrDefault();
                    DeviceDataModel lastdeviceData = _deviceDataDao.ListByNameTimeDSC(d.DeviceName, sTime.AddDays(i), sTime.AddDays(i + 1), 1).FirstOrDefault();
                    //_logger.LogInformation(deviceDatas.Count().ToString());
                    if (firstdeviceData != null)
                    {
                        double onlineTime = (lastdeviceData.Timestamp - firstdeviceData.Timestamp).TotalMinutes;
                        _logger.LogInformation("����ʱ�� " + onlineTime + " ����");
                        _logger.LogInformation("�������ݵ�DailyOnlineTime��...");
                        _deviceDailyOnlineTimeDao.InsertData(d, onlineTime, sTime.AddDays(i));
                    }
                }
            }
            /*
            List<DeviceDataModel> todayData = this._deviceDataDao.GetByDate(DateTime.Now);
            Dictionary<String, List<DateTime>> deviceKV = new Dictionary<string, List<DateTime>>(); 
            foreach (var i in todayData)
            {
                if (!deviceKV.ContainsKey(i.DeviceId))
                {
                    deviceKV.Add(i.DeviceId, new List<DateTime>());
                    deviceKV[i.DeviceId].Add(i.Timestamp);
                }
                else
                {
                    deviceKV[i.DeviceId].Add(i.Timestamp);
                }
            }
            foreach (var i in deviceKV.Keys)
            {
                DateTime first = deviceKV[i].Min();
                DateTime last = deviceKV[i].Max();
                var onlineTime = last - first;
                this._deviceDailyOnlineTimeDao.InsertData(this._deviceDao.GetByDeviceId(i), onlineTime.TotalMinutes);
            }
            */
        }
    }
}