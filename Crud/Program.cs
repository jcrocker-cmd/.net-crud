﻿using Crud.Contracts;
using Crud.Data;
using Crud.Service;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.SqlServer;
{
    
}

var builder = WebApplication.CreateBuilder(args);

//builder.WebHost.UseUrls("http://0.0.0.0:80");

// Allow CORS
// Allow CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowViteDev", policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Register your DbContext
builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Register your custom service
builder.Services.AddScoped<ISP_EmployeeService, SP_EmployeeService>();
builder.Services.AddScoped<IEmployeeJobService, EmployeeJobService>();
//builder.Services.AddScoped<SP_EmployeeService>();
// OR if using interface-based DI
// builder.Services.AddScoped<ISP_EmployeeService, SP_EmployeeService>();

// Add Hangfire services
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// Enable CORS for your React app
app.UseCors("AllowViteDev");
app.UseAuthorization();
app.MapControllers();
app.MapHub<Crud.Hubs.EmployeeHub>("/employeeHub");

// Add Hangfire dashboard
app.UseHangfireDashboard();

// Schedule the recurring job
RecurringJob.AddOrUpdate<EmployeeJobService>("add-random-employee",job => job.AddRandomEmployeeAsync(),"0 * * * *");

app.Run();
