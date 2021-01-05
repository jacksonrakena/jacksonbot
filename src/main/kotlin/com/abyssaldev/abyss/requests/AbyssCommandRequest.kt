package com.abyssaldev.abyss.requests

import com.abyssaldev.commands.gateway.GatewayCommandRequest
import net.dv8tion.jda.api.entities.Message

class AbyssCommandRequest(message: Message) : GatewayCommandRequest(message)