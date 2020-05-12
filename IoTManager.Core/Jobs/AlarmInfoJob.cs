using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IoTManager.Core.Infrastructures;
using IoTManager.IDao;
using IoTManager.Model;
using IoTManager.Utility.Serializers;
using Microsoft.Extensions.Logging;
using Pomelo.AspNetCore.TimedJob;

namespace IoTManager.Core.Jobs
{
    public class AlarmInfoJob: Job
    {
        private readonly IDeviceDataDao _deviceDataDao;
        private readonly IAlarmInfoDao _alarmInfoDao;
        private readonly IThresholdDao _thresholdDao;
        private readonly ISeverityDao _severityDao;
        private readonly IDeviceDao _deviceDao;
        private readonly IDeviceBus _deviceBus;
        private readonly IStateTypeDao _stateTypeDao;
        private readonly IFieldBus _fieldBus;
        private readonly IFieldDao _fieldDao;
        private readonly ILogger _logger;
        private readonly IDeviceDailyOnlineTimeDao _deviceDailyOnlineTimeDao;

        public AlarmInfoJob(IDeviceDataDao deviceDataDao, 
            IAlarmInfoDao alarmInfoDao, 
            IThresholdDao thresholdDao, 
            ISeverityDao severityDao, 
            IDeviceDao deviceDao,
            IDeviceBus deviceBus,
            IStateTypeDao stateTypeDao,
            IFieldBus fieldBus,
            IFieldDao fieldDao,
            IDeviceDailyOnlineTimeDao deviceDailyOnlineTimeDao,
            ILogger<AlarmInfoJob> logger)
        {
            this._deviceDataDao = deviceDataDao;
            this._alarmInfoDao = alarmInfoDao;
            this._thresholdDao = thresholdDao;
            this._severityDao = severityDao;
            this._deviceDao = deviceDao;
            this._deviceBus = deviceBus;
            this._stateTypeDao = stateTypeDao;
            this._fieldBus = fieldBus;
            this._fieldDao = fieldDao;
            this._logger = logger;
            this._deviceDailyOnlineTimeDao = deviceDailyOnlineTimeDao;
        }
        /*��ʱ�����񣺣�1���ж��豸����ֵ�Ƿ񳬳���ֵ����2���ж��豸�Ƿ�����*/
        [Invoke(Begin = "2020-4-26 11:48", Interval = 1000 * 60, SkipWhileExecuting = true)]
        public void Run()
        {
            
            
            /*
            this._logger.LogInformation("AlarmInfoJob Run ...");
            List<DeviceModel> devices = this._deviceDao.Get("all");
            //�������豸��Ϊ����״̬
            foreach (DeviceModel device in devices)
            {
                this._deviceDao.SetDeviceOnlineStatus(device.HardwareDeviceId, "yes");
            }
            //��ȡ�豸������"IsScam"="false"���������ݣ�����Ϊ"true"
            List<DeviceDataModel> dataNotInspected = _deviceDataDao.GetNotInspected();
            Dictionary<String, List<DeviceDataModel>> sortedData = new Dictionary<string, List<DeviceDataModel>>();
            Dictionary<String, List<String>> fieldMap = new Dictionary<string, List<string>>();
            List<String> deviceIds = new List<string>();
            //��deviceId��ͬ���������ϳ�sortedData,key���deviceId,value�����Ӧ�������б�
            //��deviceId��ͬ������Id���ϳ�fieldMap,key���deviceId,value�������Id���б�
            foreach (DeviceDataModel d in dataNotInspected)
            {
                if (!sortedData.ContainsKey(d.DeviceId))
                {
                    sortedData.Add(d.DeviceId, new List<DeviceDataModel>());
                    sortedData[d.DeviceId].Add(d);
                }
                else
                {
                    sortedData[d.DeviceId].Add(d);
                }

                if (!fieldMap.ContainsKey(d.DeviceId))
                {
                    fieldMap.Add(d.DeviceId, new List<string>());
                    fieldMap[d.DeviceId].Add(d.IndexId);
                }
                else
                {
                    fieldMap[d.DeviceId].Add(d.IndexId);
                }
                
                deviceIds.Add(d.DeviceId);
            }

            //Automatically add fields from device data
            foreach (String did in fieldMap.Keys)
            {
                List<FieldSerializer> affiliateFields = _fieldBus.GetAffiliateFields(did);
                List<String> affiliateFieldsId = new List<string>();
                foreach (var field in affiliateFields)
                {
                    affiliateFieldsId.Add(field.fieldId);
                }

                foreach (String fid in fieldMap[did])
                {
                    if (!affiliateFieldsId.Contains(fid))
                    {
                        FieldSerializer tmpField = new FieldSerializer();
                        tmpField.fieldName = fid;
                        tmpField.fieldId = fid;
                        DeviceModel tmpDevice = this._deviceDao.GetByDeviceId(did);
                        tmpField.device = tmpDevice.DeviceName;
                        this._fieldBus.CreateNewField(tmpField);
                    }
                }
            }
            
            deviceIds = deviceIds.Distinct().ToList();

            foreach (String did in deviceIds)
            {
                this._deviceDao.UpdateLastConnectionTimeByDeviceId(did);
            }

            foreach (DeviceModel d in devices)
            {
                DeviceTypeModel tmpDeviceType = this._stateTypeDao.GetDeviceTypeByName(d.DeviceType);
                Double tmpOfflineTime = tmpDeviceType.OfflineTime;
                TimeSpan passTime = DateTime.Now - d.LastConnectionTime.ToLocalTime();
                if (passTime.TotalMinutes > tmpOfflineTime)
                {
                    this._deviceDao.SetDeviceOnlineStatus(d.HardwareDeviceId, "no");
                }
            }
            
            Dictionary<String, String> operatorName = new Dictionary<string, string>();
            operatorName.Add("equal", "=");
            operatorName.Add("greater", ">");
            operatorName.Add("less", "<");
            foreach (String did in deviceIds)
            {
                List<ThresholdModel> thresholdDic = _thresholdDao.GetByDeviceId(did);
                foreach (DeviceDataModel data in sortedData[did])
                {
                    var query = thresholdDic.AsQueryable()
                        .Where(t => t.IndexId == data.IndexId)
                        .ToList();
                    foreach (var th in query)
                    {
                        String op = th.Operator;
                        double threshold = th.ThresholdValue;

                        Boolean abnormal = false;

                        if (op == "equal")
                        {
                            if (data.IndexValue - threshold < 0.0001)
                            {
                                abnormal = true;
                            }
                        }
                        else if (op == "less")
                        {
                            if (data.IndexValue <= threshold)
                            {
                                abnormal = true;
                            }
                        }
                        else if (op == "greater")
                        {
                            if (data.IndexValue >= threshold)
                            {
                                abnormal = true;
                            }
                        }

                        if (abnormal == true)
                        {
                            AlarmInfoModel alarmInfo = new AlarmInfoModel();
                            alarmInfo.AlarmInfo = th.Description;
                            alarmInfo.DeviceId = data.DeviceId;
                            alarmInfo.IndexId = data.IndexId;
                            alarmInfo.IndexName = data.IndexName;
                            alarmInfo.IndexValue = data.IndexValue;
                            alarmInfo.ThresholdValue = threshold;
                            alarmInfo.Timestamp = DateTime.Now;
                            alarmInfo.Severity = th.Severity;
                            alarmInfo.Processed = "No";

                            _alarmInfoDao.Create(alarmInfo);
                        }
                    }
                }
            }*/
            /* zxin-�澯��Ϣ����������жϷ�������⣺
             * �澯��Ϣ�����澯�ж�Ƶ�� 1min
             * 1����threshold���л�ȡ���и澯����
             * 2����deviceId��MonitoringId��ȡǰ1min�����ݣ��ж��Ƿ񳬹���ֵ��������ֵ������alarmInfo�����в���澯��Ϣ
             * �豸�����жϣ�Ƶ�� 1min
             * 1����MySQL��ȡ����ע���豸��Ϣ�Լ��豸��������
             * 2������deviceId��MongoDB�л�ȡ����һ�����ݣ�����뵱ǰʱ���ֵ���ڳ�ʱ�澯ʱ����Ϊ����״̬���豸״̬���£�
             */
            _logger.LogInformation("AlarmInfoJob Run...");
            _logger.LogInformation("�澯�ж�...");
            /*�澯�ж�*/
            //��ȡ���и澯���򣬲���澯��Ϣ��MongoDB
            List<ThresholdModel> alarmRules = this._thresholdDao.Get("all");
            foreach(var rule in alarmRules )
            {
                //��ȡ���и澯���������60����豸����
                List<DeviceDataModel> deviceDatas = this._deviceDataDao.ListNewData(rule.DeviceId,60, rule.IndexId);
                //var isAlarmInfo=deviceDatas.AsQueryable()
                //    .Where(dd=>dd.IndexValue>rule.ThresholdValue)
                foreach(var dd in deviceDatas)
                {
                    AlarmInfoModel alarmInfo = AlarmInfoGenerator(dd, rule);
                    if(alarmInfo!=null)
                    {
                        _alarmInfoDao.Create(alarmInfo);
                    }
                    
                }
            }
            

            _logger.LogInformation("�豸�����ж�...");
            /*�豸�����ж�*/
            List<DeviceModel> devices = this._deviceDao.Get("all");
            List<DeviceTypeModel> deviceTypes = this._stateTypeDao.ListAllDeviceType();
            Dictionary<String, List<String>> fieldMap = new Dictionary<string, List<string>>();
            foreach (var device in devices)
            {
                //��ȡ��ʱ�澯ʱ�䣨���ӣ�
                double offlinetime = deviceTypes.AsQueryable().Where(dt => dt.DeviceTypeName == device.DeviceType).FirstOrDefault().OfflineTime;
                DateTime date = DateTime.Now - TimeSpan.FromMinutes(offlinetime);
                //��ȡ����һ���豸����
                DeviceDataModel deviceData = this._deviceDataDao.GetByDeviceName(device.DeviceName, 1).FirstOrDefault();
                if(deviceData!=null)
                {
                    if(deviceData.Timestamp>=date)
                    {
                        this._deviceDao.SetDeviceOnlineStatus(device.HardwareDeviceId, "yes");//�豸����
                    }
                    else
                    {
                        this._deviceDao.SetDeviceOnlineStatus(device.HardwareDeviceId, "no");
                    }
                }
                else
                {
                    this._deviceDao.SetDeviceOnlineStatus(device.HardwareDeviceId, "no");
                }

                /* �����豸���ԣ�
                 * 1���г�MySQL����������
                 * 2����ȡMongoDB�����������е�����
                 * 3���ȶԲ�����������
                 */
                List<string> existedFieldIds = this._fieldDao.ListFieldIdsByDeviceId(device.HardwareDeviceId);
                List<DeviceDataModel> deviceDatas= this._deviceDataDao.ListNewData(device.HardwareDeviceId, 60);
                foreach(var dd in deviceDatas)
                {
                    if(!existedFieldIds.Contains(dd.IndexId))
                    {
                        FieldModel field = new FieldModel
                        {
                            FieldId = dd.IndexId,
                            FieldName = dd.IndexName,
                            Device = dd.DeviceName
                        };
                        this._fieldDao.Create(field);
                    }
                }
                /*�����豸���ܸ澯����*/
                //��ȡ�豸��ǰ�ܵĸ澯����
                //int totalInfo = _alarmInfoDao.GetDeviceAffiliateAlarmInfoNumber(device.HardwareDeviceId);
                //int totalInfo = 0;
                //����MySQL�е��豸�ĸ澯�ܴ���
                //_deviceBus.UpdateTotalAlarmInfo(device.HardwareDeviceId, totalInfo);
            }
        }
        
        public AlarmInfoModel AlarmInfoGenerator(DeviceDataModel deviceData, ThresholdModel threshold)
        {
            AlarmInfoModel alarmInfo = new AlarmInfoModel
            {
                AlarmInfo = threshold.Description,
                DeviceId = deviceData.DeviceId,
                IndexId = deviceData.IndexId,
                IndexName = deviceData.IndexName,
                IndexValue = deviceData.IndexValue,
                ThresholdValue = threshold.ThresholdValue,
                Timestamp = DateTime.Now,
                Severity = threshold.Severity,
                Processed = "No"
            };
            if(threshold.Operator== "equal")
            {
                if(deviceData.IndexValue!=threshold.ThresholdValue)
                {
                    return alarmInfo;
                }
            }
            else if(threshold.Operator=="less")
            {
                if(deviceData.IndexValue>threshold.ThresholdValue)
                {
                    return alarmInfo;
                }
            }
            else
            {
                if(deviceData.IndexValue<threshold.ThresholdValue)
                {
                    return alarmInfo;
                }
            }
            return null;
        }
        public void ReportJob()
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
                double totalDays = deviceData != null ? (nowadays - new DateTime(deviceData.Timestamp.Year, deviceData.Timestamp.Month, deviceData.Timestamp.Day)).TotalDays
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