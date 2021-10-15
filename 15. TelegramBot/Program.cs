using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;


var botClient = new TelegramBotClient("2072998845:AAG8ylM1aTOaIi7lyfBQuU04r_kbX-OSiu8");

var me = await botClient.GetMeAsync();

List<Room> Rooms = new List<Room>();
Room lastroom = new Room();//ой костыли мои костылятские

Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");

using var cts = new CancellationTokenSource();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
botClient.StartReceiving(
    new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync),
    cts.Token);

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();

Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    if (update.Type != UpdateType.Message)
        return;
    if (update.Message.Type != MessageType.Text)
        return;
    
    var chatId = update.Message.Chat.Id;

    Console.WriteLine($"Received a '{update.Message.Text}' message in chat {chatId}.");

    if(update.Message.Text == "N" || update.Message.Text == "n"){//Когда-нибудь этот код будет предлагать другие варианты комнат, но у нас MVP, поэтому он просто опять принимает на ввод время
        await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: "Ну блин, у нас MVP, пока что придется ввести другое время"
    );
    Console.WriteLine("Wrote: " + "Ну блин, у нас MVP, пока что придется ввести другое время" + " in " + chatId);
        return;
    }

    User user = new User(update.Message.From);

    if (update.Message.Text == "Y" || update.Message.Text == "y"){
        if(lastroom.QuantityOfRoommates > 0){
            await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Вот контакты Ваших попутчиков:\n" + lastroom.RoommatesContacts()
            );
            Console.WriteLine("Wrote: " + "Вот контакты Ваших попутчиков:\n" + lastroom.RoommatesContacts() + " in " + chatId);
        }
        else{
            await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Вы пока один, ожидайте появления попутчиков"
            );
            Console.WriteLine("Wrote: " + "Вы пока один, ожидайте появления попутчиков" + " in " + chatId);
        }
        foreach(User userOld in lastroom.Roommates){
            await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Ваш новый попутчик:\n" + user.ChatId.Username
            );;
            Console.WriteLine("Wrote: " + "Ваш новый попутчик:\n" + user.ChatId.Username + " in " + chatId);
        }
        await lastroom.AddRoommate(user, botClient);
        return;
    }

    DateTime timeWanted = new DateTime();

    try{
        timeWanted = DateTime.Parse(update.Message.Text);
    }
    catch{
        await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: "А шо происходит? Я не понимаю. Введите время!"
        );
        Console.WriteLine("Wrote: " + "А шо происходит? Я не понимаю. Введите время!" + " in " + chatId);
    }
    if (Rooms.Exists(element => timeWanted <= element.EndTime && timeWanted >= element.StartTime)){
        Room roomIn = Rooms.Find(element => timeWanted <= element.EndTime && timeWanted >= element.StartTime);
        lastroom = roomIn;
        await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: "Вам подходит эта комната?\n" + "Количество людей: " + roomIn.QuantityOfRoommates + "/4\n" + "Примерное время отъезда: " + roomIn.StartTime + " - " + roomIn.EndTime + "\n" + "(Y/N)"
        );
        Console.WriteLine("Wrote: " + "Вам подходит эта комната?\n" + "Количество людей: " + roomIn.QuantityOfRoommates + "/4\n" + "Примерное время отъезда: " + roomIn.StartTime + " - " + roomIn.EndTime + "\n" + "(Y/N)" + " in " + chatId);
        return;
    }

    
    Room room = new Room(timeWanted);
    lastroom = room;
    Rooms.Add(lastroom);
    

    await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: "Вам подходит эта комната?\n" + "Количество людей: " + room.QuantityOfRoommates + "/4\n" + "Примерное время отъезда: " + room.StartTime + " - " + room.EndTime + "\n" + "(Y/N)"
    );
    Console.WriteLine("Wrote: " + "Вам подходит эта комната?\n" + "Количество людей: " + room.QuantityOfRoommates + "/4\n" + "Примерное время отъезда: " + room.StartTime + " - " + room.EndTime + "\n" + "(Y/N)" + " in " + chatId);
}
