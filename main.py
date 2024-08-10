import os

import discord
from discord.ext import commands

intents = discord.Intents.all()
intents.members = True
intents.message_content = True

presence = discord.Game(name='com a Madoka')

bot = commands.Bot(command_prefix='?', activity=presence, intents=intents)


@bot.event
async def on_ready():
    print(f'Logado como: {bot.user}')
    print('------------------------')


token = os.environ['TOKEN']
bot.run(token)
