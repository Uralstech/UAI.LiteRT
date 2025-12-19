// Copyright 2025 URAV ADVANCED LEARNING SYSTEMS PRIVATE LIMITED
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

package com.uralstech.uai.litertlm

import android.util.Log
import com.google.ai.edge.litertlm.Content
import com.google.ai.edge.litertlm.Conversation
import com.google.ai.edge.litertlm.Message
import com.google.ai.edge.litertlm.MessageCallback

class ConversationWrapper(private val conversation: Conversation) {

    companion object {
        private const val TAG = "ConversationWrapper"

        @JvmStatic
        fun messageOf(contents: ArrayList<Content>) : Message {
            return Message.of(contents)
        }

        @JvmStatic
        fun messageOf(content: String) : Message {
            return Message.of(content)
        }
    }

    fun sendMessage(message: Message) : Message? {
        if (!checkInitForConvo()) return null
        Log.i(TAG, "Sending message (sync).")

        try {
            val result = conversation.sendMessage(message)

            Log.i(TAG, "Conversation turn completed.")
            return result
        } catch (ex: RuntimeException) {
            Log.e(TAG, "Could not send message due to exception", ex)
            return null
        }
    }

    fun sendMessageAsync(message: Message, callbacks: MessageCallback) : Boolean {
        if (!checkInitForConvo()) return false
        Log.i(TAG, "Sending message (async).")

        return try {
            conversation.sendMessageAsync(message, callbacks)
            true
        } catch (ex: IllegalStateException) {
            Log.e(TAG, "Could not send message due to exception", ex)
            false
        }
    }

    fun cancelProcess() : Boolean {
        if (!checkInitForConvo()) return false

        conversation.cancelProcess()
        Log.i(TAG, "Process cancelled.")
        return true
    }

    private fun checkInitForConvo() : Boolean {
        if (!conversation.isAlive) {
            Log.e(TAG, "Tried to modify dead conversation!")
            return false
        }

        return true
    }

    fun close() {
        if (conversation.isAlive) {
            conversation.close()
            Log.i(TAG, "Conversation closed.")
        }
    }
}