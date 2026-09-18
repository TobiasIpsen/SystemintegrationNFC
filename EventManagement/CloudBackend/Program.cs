
using Amazon.S3;
using CloudBackend.Database;
using CloudBackend.RabbitMQ;
using CloudBackend.Service;
using Microsoft.EntityFrameworkCore;

namespace CloudBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string MyCorsPolicy = "MyPolicy";

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: MyCorsPolicy, policy =>
                    {
                        policy.AllowAnyOrigin() //WithOrigins("http://localhost:5173/")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    });
            });

            var seaweedConfig = builder.Configuration.GetSection("SeaweedFS");
            builder.Services.AddSingleton<IAmazonS3>(sp =>
            {
                var config = new AmazonS3Config
                {
                    ServiceURL = seaweedConfig["ServiceUrl"],
                    ForcePathStyle = true,
                    UseHttp = true
                };

                return new AmazonS3Client(
                    seaweedConfig["AccessKey"],
                    seaweedConfig["SecretKey"],
                    config
                );
            });

            builder.Services.AddDbContext<CloudDb>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddScoped<IStudentService, StudentService>();
            builder.Services.AddScoped<IEventService, EventService>();

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddSingleton<UserMessaging>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseCors(MyCorsPolicy);

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
