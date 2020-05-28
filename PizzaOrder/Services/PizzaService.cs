using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using PizzaOrder.Models;
using Microsoft.Extensions.Configuration;

namespace PizzaOrder.Services
{
    public class PizzaService
    {

        private IConfiguration Configuration { get; set; }
        public PizzaService(IConfiguration config)
        {
            Configuration = config;
        }

        public string GetConnectionString()
        {
            return Configuration.GetValue<string>("ConnectionStrings:pizzaDbConnection");
        }

        public IEnumerable<Pizza> GetPizza()
        {
            var pizza = new List<Pizza>();
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                try
                {
                    connection.Open();
                    string query = "select * from Pizza with(nolock)";
                    var command = new SqlCommand(query, connection);
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            pizza.Add(new Pizza()
                            {
                                Id = dataReader.GetInt32(dataReader.GetOrdinal("Id")),
                                PizzaName = dataReader.GetString(dataReader.GetOrdinal("PizzaName")),
                                Price = dataReader.GetInt32(dataReader.GetOrdinal("Price"))
                            });
                        }
                    }
                }
                catch (Exception ex)
                {

                }

            }
            return pizza;
        }

        public IEnumerable<Pizza> GetOrders(string username)
        {
            var pizza = new List<Pizza>();
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                try
                {
                    connection.Open();
                    string query = "select p.Id,p.PizzaName,p.Price  from pizza p inner join [orderItem] oi on oi.PizzaId = p.Id inner join [order] o on o.OrderCode = oi.OrderCode where o.UserName = " + "'" + username + "'" + ";";
                    SqlCommand command = new SqlCommand(query, connection);
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            pizza.Add(new Pizza()
                            {
                                Id = dataReader.GetInt32(dataReader.GetOrdinal("Id")),
                                PizzaName = dataReader.GetString(dataReader.GetOrdinal("PizzaName")),
                                Price = dataReader.GetInt32(dataReader.GetOrdinal("Price"))
                            });
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
            return pizza;
        }

        public string GetOrderCode(string orderCode)
        {
            return orderCode.Substring(0, 3) + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond;
        }

        public void CreateOrder(PizzaOrders orders)
        {
            var requestedOrders = orders.OrderItems;
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();
                var orderCode = GetOrderCode(orders.UserName);
                string orderQuery = "insert into [order] values(" + "'" + orders.UserName + "'" + "," + "'" + orderCode + "'" + ");";
                SqlCommand command1 = new SqlCommand(orderQuery, connection, transaction);
                SqlCommand command2 = null;
                try
                {
                    command1.ExecuteNonQuery();
                    foreach (var item in requestedOrders)
                    {
                        string orderItemQuery = "insert into [orderItem] values(" + item.Id + "," + "'" + orderCode + "'" + ");";
                        command2 = new SqlCommand(orderItemQuery, connection, transaction);
                        command2.ExecuteNonQuery();
                    }
                    Commit(transaction);
                }
                catch (Exception ex)
                {
                    Rollback(transaction);
                }
            }
        }

        public void UpdateOrder(PizzaOrders orders)
        {
            var listedId = new List<OrderItems>();
            var requestedOrders = orders.OrderItems;
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                string orderUpdatequery = "update [order] set username=" + "'" + orders.UserName + "'" + " where OrderCode =" + "'" + orders.OrderCode + "'" + ";";
                SqlCommand command1 = new SqlCommand(orderUpdatequery, connection, transaction);

                string oldItems = "select Id from [orderItem] with(nolock) where OrderCode = " + "'" + orders.OrderCode + "'" + " ;";
                SqlCommand command2 = new SqlCommand(oldItems, connection, transaction);


                using (SqlDataReader dataReader = command2.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        listedId.Add(new OrderItems()
                        {
                            Id = dataReader.GetInt32(dataReader.GetOrdinal("Id"))
                        });
                    }
                }

                try
                {
                    command1.ExecuteNonQuery();
                    var records = listedId.Zip(requestedOrders, (n, w) => new { OrderItemIds = n, updatedRecords = w });
                    foreach (var item in records)
                    {
                        string updateOrderItem = "update [orderItem] set pizzaId = " + item.updatedRecords.Id + " where Id = " + item.OrderItemIds.Id + "";
                        SqlCommand command3 = new SqlCommand(updateOrderItem, connection, transaction);
                        command3.ExecuteNonQuery();
                    }
                    Commit(transaction);
                }
                catch (Exception ex)
                {
                    Rollback(transaction);
                }
            }
        }

        public void DeleteOrder(string orderCode)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                string deleteOrderQuery = "Delete [order] where OrderCode = " + "'" + orderCode + "'" + ";";
                string deleteOrderItemQuery = "Delete [orderItem] where OrderCode = " + "'" + orderCode + "'" + ";";

                SqlCommand command1 = new SqlCommand(deleteOrderQuery, connection, transaction);
                SqlCommand command2 = new SqlCommand(deleteOrderItemQuery, connection, transaction);
                try
                {
                    command1.ExecuteNonQuery();
                    command2.ExecuteNonQuery();
                    Commit(transaction);
                }
                catch (Exception ex)
                {
                    Rollback(transaction);
                }
            }
        }


        public void Rollback(SqlTransaction transaction)
        {
            transaction.Rollback();
        }

        public void Commit(SqlTransaction transaction)
        {
            transaction.Commit();
        }
    }
}
