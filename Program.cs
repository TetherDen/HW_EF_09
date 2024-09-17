using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace hw_09
{
    internal class Program
    {
        //Создать таблицы: «Станция» и «Поезд». Используя метод FromSqlRaw и ExecuteSqlRaw, выполнить 8 запросов для получения данных:

        //    1. Добавить данные про станции и поезда.
        //    2. Поезда у которых длительность маршрута более 5 часов.
        //    3. Общую информация о станции и ее поездах.
        //    4. Название станций у которой в наличии более 3-ех поездов.
        //    5. Все поезда, модель которых начинается на подстроку «Pell».
        //    6. Все поезда, у которых возраст более 15 лет с текущей даты.
        //    7. Получить станции, у которых в наличии хотя бы один поезд с длительность маршрутка менее 4 часов.
        //    8. Вывести все станции без поездов (на которых не будет поездов при выполнении LEFT JOIN).

        static void Main(string[] args)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }

            using(var db = new ApplicationContext())
            {
                //    1. Добавить данные про станции и поезда.

                db.Database.ExecuteSqlInterpolated($"INSERT INTO [Stations] ([Name], [Description]) VALUES ('StationNameOne', 'DescriptionStationOne')");
                db.Database.ExecuteSqlInterpolated($@"
                    INSERT INTO [Stations] ([Name], [Description]) 
                    VALUES ('ThirdStation', 'NewDescription333')");
                db.Database.ExecuteSqlInterpolated($@"
                    INSERT INTO [Stations] ([Name], [Description]) 
                    VALUES ('randomStation', 'RandomDescription')");


                db.Database.ExecuteSqlInterpolated($@"
                    INSERT INTO [Trains] ([TrainName], [RouteDuration], [ProductionDate], [StationId])
                    VALUES ('trName1',3,'2020-01-01',1);

                    INSERT INTO [Trains] ([TrainName], [RouteDuration], [ProductionDate], [StationId])
                    VALUES ('Train2',4,'2005-01-01',2);

                    INSERT INTO [Trains] ([TrainName], [RouteDuration], [ProductionDate], [StationId])
                    VALUES ('Train3Pell',5,'2000-01-01',1);

                    INSERT INTO [Trains] ([TrainName], [RouteDuration], [ProductionDate], [StationId])
                    VALUES ('PellTrain4',6,'2001-01-01',2);
                    ");
            }

            using (var db = new ApplicationContext())
            {
                //    2. Поезда у которых длительность маршрута более 5 часов.    
                var trains = db.Trains.FromSqlInterpolated($"SELECT * FROM [Trains] WHERE [RouteDuration] >= 5").ToList();


                //    3. Общую информация о станции и ее поездах.
                var stParam= new SqlParameter("@Name", "StationNameOne");
                var station = db.Stations.FromSqlRaw("SELECT * FROM [Stations] WHERE [Name] = @Name", stParam).Include(c => c.Trains).FirstOrDefault();


                //    4. Название станций у которой в наличии более 3-ех поездов.
                var task4 = db.Stations.FromSqlInterpolated($@"
                    SELECT s.* FROM [Stations] as s 
                    WHERE ( 
                        SELECT COUNT(*) FROM [Trains] as t
                        WHERE t.StationId = s.Id ) >1 ").ToList();


                //    5. Все поезда, модель которых начинается на подстроку «Pell».
                string subString = "Pell%";

                var task5 = db.Trains.FromSqlInterpolated($@"
                    SELECT * FROM [Trains] 
                    WHERE [TrainName] LIKE  {subString}").ToList();


                //    6. Все поезда, у которых возраст более 15 лет с текущей даты.
                var task6 = db.Trains.FromSqlInterpolated($@"SELECT * FROM [Trains] WHERE DATEDIFF(YEAR, [ProductionDate], GETDATE()) > 15").ToList();


                ////    7. Получить станции, у которых в наличии хотя бы один поезд с длительность маршрутка менее 4 часов.
                var task7 = db.Stations.FromSqlInterpolated($@"
                    SELECT s.* FROM [Stations] as s
                    WHERE ( 
                        SELECT COUNT(*) FROM [Trains] as t
                        WHERE t.StationId = s.Id AND t.RouteDuration <= 4 ) >= 1 ").ToList();


                //8.Вывести все станции без поездов(на которых не будет поездов при выполнении LEFT JOIN).
                var task8 = db.Stations.FromSqlInterpolated($@"
                    SELECT s.Id, s.Name, s.Description FROM [Stations] as s
                    LEFT JOIN [Trains] as t ON t.StationId = s.Id 
                    WHERE t.StationId IS NULL
                    ").ToList();


            }
        }
    }
}

public class Station
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<Train> Trains { get; set; }
}

public class Train
{
    public int Id { get; set; }
    public string TrainName { get; set; }
    public int RouteDuration { get; set; }
    public DateTime ProductionDate { get; set; }

    public int StationId { get; set; }
    public Station Station { get; set; }

}






public class ApplicationContext : DbContext
{
    public DbSet<Station> Stations { get; set; }
    public DbSet<Train> Trains { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=DESKTOP-V6G1V7P;Database=test_db;Trusted_Connection=True;TrustServerCertificate=True;"); // ПК
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder);
    }
}