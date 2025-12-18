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

package com.uralstech.uai.litert

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

    interface AsyncInferenceCallbacks {
        fun onDone()
        fun onError(message: String?)
        fun onMessage(content: String)
    }

    fun sendMessage(message: Message) : String? {
        if (!checkInitForConvo()) return null
        Log.i(TAG, "Sending message (sync).")

        val result = conversation.sendMessage(message).toString()
        Message

        Log.i(TAG, "Conversation turn completed.")
        return result
    }

    fun sendMessageAsync(message: Message, callbacks: AsyncInferenceCallbacks) : Boolean {
        if (!checkInitForConvo()) return false
        Log.i(TAG, "Sending message (async).")

        conversation.sendMessageAsync(message, object : MessageCallback {
            override fun onMessage(message: Message) {
                callbacks.onMessage(message.toString())
            }

            override fun onDone() {
                Log.i(TAG, "Conversation turn completed.")
                callbacks.onDone()
            }

            override fun onError(throwable: Throwable) {
                Log.e(TAG, "An exception occurred during the conversation", throwable)
                callbacks.onError(throwable.message)
            }
        })

        return true;
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