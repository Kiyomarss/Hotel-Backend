﻿using System.Text;
using ContactsManager.Core.Domain.IdentityEntities;
using Hotel_Infrastructure.DbContext;
using Hotel_Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using Repositories;
using RepositoryContracts;
using ServiceContracts;
using Services;

namespace Hotel_UI
{
 public static class ConfigureServicesExtension
 {
  public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
  {
   services.AddControllers();
   
   //add services into IoC container
   services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
   
   services.AddScoped<IBookingsRepository, BookingsRepository>();
   services.AddScoped<IBookingsGetterService, BookingsGetterService>();
   services.AddScoped<IBookingsDeleterService, BookingsDeleterService>();
   services.AddScoped<IBookingsUpdaterService, BookingsUpdaterService>();

   services.AddScoped<ICabinsRepository, CabinsRepository>();
   services.AddScoped<ICabinsGetterService, CabinsGetterService>();
   services.AddScoped<ICabinsDeleterService, CabinsDeleterService>();
   services.AddScoped<ICabinsUpdaterService, CabinsUpdaterService>();
   services.AddScoped<ICabinsAdderService, CabinsAdderService>();
   
   services.AddScoped<ISettingRepository, SettingRepository>();
   services.AddScoped<ISettingGetterService, SettingGetterService>();
   services.AddScoped<ISettingUpdaterService, SettingUpdaterService>();

   services.AddDbContext<ApplicationDbContext>(options =>
   {
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
   });

   services.AddAuthentication(options =>
    {
     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
     options.TokenValidationParameters = new TokenValidationParameters
     {
      ValidateIssuer = true,
      ValidateAudience = true,
      ValidateLifetime = true,
      ValidateIssuerSigningKey = true,
      ValidIssuer = "YourIssuerHere",
      ValidAudience = "YourAudienceHere",
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourSecretKeyHere"))
     };
    });
   
   services.AddEndpointsApiExplorer();

   services.AddApiVersioning(options =>
   {
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
   });

   services.AddResponseCompression(options =>
   {
    options.EnableForHttps = true;
   });
   
   services.AddCors(options =>
   {
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
     builder.WithOrigins("http://localhost:3090")
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials();
    });
   });

   
   services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
     options.SerializerSettings.ContractResolver = new DefaultContractResolver
     {
      NamingStrategy = new CamelCaseNamingStrategy()
     };
    });
   
   services.AddIdentity<ApplicationUser, ApplicationRole>(options => {
     options.Password.RequiredLength = 5;
     options.Password.RequireNonAlphanumeric = false;
     options.Password.RequireUppercase = false;
     options.Password.RequireLowercase = true;
     options.Password.RequireDigit = false;
     options.Password.RequiredUniqueChars = 3; //Eg: AB12AB (unique characters are A,B,1,2)
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()

    .AddDefaultTokenProviders()

    .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()

    .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>();
   
   services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
     options.Cookie.HttpOnly = true;
     options.LoginPath = "/Account/Login";
     options.LogoutPath = "/Account/Logout";
     options.SlidingExpiration = true;
    });
   
   services.AddHttpLogging(options =>
   {
    options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestProperties | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;
   });

   return services;
  }
 }
}
