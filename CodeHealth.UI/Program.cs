﻿using Microsoft.AspNetCore.Components.Web;
using BlazorDesktop.Hosting;
using CodeHealth.UI.Components;

var builder = BlazorDesktopHostBuilder.CreateDefault(args);

builder.RootComponents.Add<Routes>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

if (builder.HostEnvironment.IsDevelopment())
{
    builder.UseDeveloperTools();
}

var app = builder.Build();

await app.RunAsync();