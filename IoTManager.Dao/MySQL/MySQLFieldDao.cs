using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using IoTManager.IDao;
using IoTManager.Model;
using IoTManager.Utility;
using MySql.Data.MySqlClient;

namespace IoTManager.Dao
{
    public sealed class MySQLFieldDao : IFieldDao
    {
        public List<FieldModel> Get()
        {
            using (var connection = new MySqlConnection(Constant.getDatabaseConnectionString()))
            {
                List<FieldModel> fields = connection.Query<FieldModel>(
                    "select fieldName, fieldId, deviceName device from field join device on field.device=device.id").ToList();
                return fields;
            }
        }
        public String Create(FieldModel field)
        {
            using (var connection = new MySqlConnection(Constant.getDatabaseConnectionString()))
            {
                DeviceModel device = connection.Query<DeviceModel>("select device.id, " +
                                                                   "hardwareDeviceID, " +
                                                                   "deviceName, " +
                                                                   "city.cityName as city, " +
                                                                   "factory.factoryName as factory, " +
                                                                   "workshop.workshopName as workshop, " +
                                                                   "deviceState, " +
                                                                   "device.imageUrl, " +
                                                                   "gateway.gatewayName gatewayId, " +
                                                                   "mac, " +
                                                                   "deviceType, " +
                                                                   "device.remark, " +
                                                                   "device.lastConnectionTime, " +
                                                                   "device.createTime, " +
                                                                   "device.updateTime " +
                                                                   "from device " +
                                                                   "join city on city.id=device.city " +
                                                                   "join factory on factory.id=device.factory " +
                                                                   "join workshop on workshop.id=device.workshop " +
                                                                   "join gateway on gateway.id=device.gatewayId " + 
                                                                   "where deviceName=@dn", new {dn=field.Device})
                    .FirstOrDefault();
                int rows = connection.Execute("insert into field(fieldName, fieldId, device) values (@fn, @fi, @d)", new
                {
                    fn = field.FieldName,
                    fi = field.FieldId,
                    d = device.Id
                });
                return rows == 1 ? "success" : "error";
            }
        }
    }
}