using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using CommunityToolkit.Maui;
using Plugin.Maui.Audio;
using Microsoft.Extensions.DependencyInjection;

namespace TicTacToe;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
            });

        // Correção aqui: Mapeia explicitamente a interface IAudioManager para o AudioManager.Current
        builder.Services.AddSingleton<IAudioManager>(AudioManager.Current);

        // Registra a MainPage como Transient (boa prática para ciclo de vida de páginas no MAUI)
        builder.Services.AddTransient<MainPage>();

        return builder.Build();
    }
}