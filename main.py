import os

import discord
from discord.ext import commands

presence = discord.Game(name='com a Madoka')

bot = commands.Bot(command_prefix='?',
                   activity=presence,
                   intents=discord.Intents.all())


@bot.event
async def on_ready():
    print(f'Logado como: {bot.user}')
    print('------------------------')
    await bot.tree.sync()


"""
# Comando base old

@bot.command()
async def teste1(ctx):
    await ctx.send('teste2')
"""


# App command teste
@bot.tree.command(name='teste-input',
                  description='Apenas um teste de app command')
async def testeinput(interaction: discord.Interaction):
    await interaction.response.send_message('Teste')


token = os.environ['TOKEN']
bot.run(token)
