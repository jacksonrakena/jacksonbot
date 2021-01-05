package com.abyssaldev.abyss.framework.interactions

import com.abyssaldev.rowi.core.CommandRequest
import com.abyssaldev.rowi.core.CommandResponse
import net.dv8tion.jda.api.MessageBuilder

class InteractionCommandResponse(isSuccess: Boolean, reason: String, val message: MessageBuilder): CommandResponse(isSuccess, reason) {
    override fun completeResponse(request: CommandRequest) {
        (request as InteractionCommandRequest).channel.sendMessage(message.build()).queue()
    }
}