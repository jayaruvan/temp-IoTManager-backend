﻿using IoTManager.IDao;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using Dapper;
using IoTManager.Model;
using IoTManager.Utility;
using MySql.Data.MySqlClient;
using Constant = IoTManager.Utility.Constant;

namespace IoTManager.Dao
{
    public sealed class MySQLCityDao : ICityDao
    {
        public String Create(CityModel cityModel)
        {
            using (var connection = new MySqlConnection(Constant.getDatabaseConnectionString()))
            {
                int rows = connection.Execute("INSERT INTO city(cityName, remark, longitude, latitude) VALUES (@cn, @r, @lo, @la)", new
                {
                    cn = cityModel.CityName,
                    r = cityModel.Remark,
                    lo = cityModel.longitude,
                    la = cityModel.latitude
                });
                return rows == 1 ? "success" : "error";
            }
        }

        public List<CityModel> Get()
        {
            using (var connection = new MySqlConnection(Constant.getDatabaseConnectionString()))
            {
                return connection
                    .Query<CityModel>("SELECT * FROM city")
                    .ToList();
            }
        }

        public CityModel GetById(int id)
        {
            using (var connection = new MySqlConnection(Constant.getDatabaseConnectionString()))
            {
                return connection
                    .Query<CityModel>("SELECT * FROM city WHERE id = @cityId",
                        new
                        {
                            cityId = id
                        }).FirstOrDefault();
            }
        }

        public String Update(int id, CityModel cityModel)
        {
            using (var connection = new MySqlConnection(Constant.getDatabaseConnectionString()))
            {
                int rows = connection.Execute("UPDATE city SET cityName=@cn, remark=@r, updateTime=CURRENT_TIMESTAMP WHERE id=@cityId", new
                {
                    cn = cityModel.CityName,
                    r = cityModel.Remark,
                    cityId = cityModel.Id
                });
                return rows == 1 ? "success" : "error";
            }
        }

        public String Delete(int id)
        {
            using (var connection = new MySqlConnection(Constant.getDatabaseConnectionString()))
            {
                int rows = connection.Execute("DELETE FROM city WHERE id=@cityId", new
                {
                    cityId = id
                });
                return rows == 1 ? "success" : "error";
            }
        }
    }
}