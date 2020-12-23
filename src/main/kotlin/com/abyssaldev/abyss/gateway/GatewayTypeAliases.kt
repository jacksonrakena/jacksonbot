package com.abyssaldev.abyss.gateway

import com.jagrosh.jdautilities.command.Command
import com.jagrosh.jdautilities.command.CommandClient
import com.jagrosh.jdautilities.command.CommandClientBuilder
import com.jagrosh.jdautilities.command.GuildSettingsManager

typealias GatewayCommand = Command
typealias GatewayCommandClient = CommandClient
typealias GatewayGuildSettingsManager<T> = GuildSettingsManager<T>
typealias GatewayCommandClientBuilder = CommandClientBuilder