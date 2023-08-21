﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;
using WebAPI_tutorial_recursos.Context;
using WebAPI_tutorial_recursos.Filters;
using WebAPI_tutorial_recursos.Middlewares;
using WebAPI_tutorial_recursos.Repository;
using WebAPI_tutorial_recursos.Repository.Interfaces;
using WebAPI_tutorial_recursos.Services;
using WebAPI_tutorial_recursos.Utilities;
using WebAPI_tutorial_recursos.Utilities.HATEOAS;

namespace WebAPI_tutorial_recursos
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            // Limpia los mapeos de los tipos de los Claims (del login de usuario)
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // Clase: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/27047628#notes
            Configuration = configuration;
        }

        /// <summary>
        /// Configuración de los Services
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                // Clase: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/13816116#notes
                options.Filters.Add(typeof(ExceptionFilter));
                options.Conventions.Add(new SwaggerGroupByVersion());
            }
            ).AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles); // para arreglar errores de loop de relaciones 1..n y viceversa
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAPI_tutorial_recursos", Version = "v1" });
                c.SwaggerDoc("v2", new OpenApiInfo { Title = "WebAPI_tutorial_recursos", Version = "v2" });
                c.OperationFilter<AddParamHATEOAS>();

                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference=new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        new string[]{ }
                    }
                });
            });

            // Configuración de la base de datos
            //var isLocalConnectionString = Configuration.GetValue<bool>("ConnectionStrings:ConnectionString_isLocal");
            //var connectionStringKey = isLocalConnectionString ? "ConnectionString_WebAPI_tutorial_local" : "ConnectionString_WebAPI_tutorial_remote";
            services.AddDbContext<ContextDB>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("ConnectionString_WebAPI_tutorial"));
            });

            services.AddAutoMapper(typeof(Startup));

            // Registro de servicios 
            // --------------

            // AddTransient: cambia dentro del contexto
            // AddScoped: se mantiene dentro del contexto (mejor para los servicios)
            // AddSingleton: no cambia nunca
            services.AddScoped<IAuthorRepository, AuthorRepository>();
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();

            // --------------

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(Configuration["KeyJwt"])),
                    ClockSkew = TimeSpan.Zero
                });

            // Identity Clase: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/27047608#notes
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ContextDB>()
                .AddDefaultTokenProviders();

            // Autorización basada en Claims
            // Agregar los roles del sistema
            // Clase: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/27047710#notes
            services.AddAuthorization(options =>
            {
                options.AddPolicy("IsAdmin", policy => policy.RequireClaim("IsAdmin"));
            });

            // Configuración CORS: para permitir recibir peticiones http desde un origen específico
            // CORS Sólo sirve para aplicaciones web (Angular, React, etc)
            // Clase: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/27047732#notes
            // apirequest.io
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins("https://apirequest.io").AllowAnyMethod().AllowAnyHeader();
                });
            });

            // Clase: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/27148834#notes
            services.AddTransient<GenerateLinks>();
            services.AddTransient<HATEOASAuthorFilterAttribute>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

        }

        /// <summary>
        /// Configuración del Middleware
        /// Middlewares son los métodos "Use..()"
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            // Middleware customizado: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/26839760#notes
            app.UseLogResponseHTTP();

            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => { 
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPI_tutorial_recursos v1");
                    c.SwaggerEndpoint("/swagger/v2/swagger.json", "WebAPI_tutorial_recursos v2");
                });
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors(); // Clase: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/27047732#notes
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

    }
}