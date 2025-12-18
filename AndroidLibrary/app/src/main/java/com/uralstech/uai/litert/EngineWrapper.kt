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

import android.os.Environment
import android.util.Log
import com.google.ai.edge.litertlm.Backend
import com.google.ai.edge.litertlm.ConversationConfig
import com.google.ai.edge.litertlm.Engine
import com.google.ai.edge.litertlm.EngineConfig
import com.google.ai.edge.litertlm.Message
import com.google.ai.edge.litertlm.SamplerConfig
import com.unity3d.player.UnityPlayer
import java.util.concurrent.Executors

class EngineWrapper private constructor(modelPath: String, backend: Backend, visionBackend: Backend?, audioBackend: Backend?, maxTokens: Int, useExternalCacheDir: Boolean) {

    companion object {
        private const val TAG = "EngineWrapper"

        private const val BACKEND_CPU = 0
        private const val BACKEND_GPU = 1
        private const val BACKEND_NPU = 2

        private fun toBackend(intVal: Int) : Backend? {
            return when (intVal)
            {
                BACKEND_CPU -> Backend.CPU
                BACKEND_GPU -> Backend.GPU
                BACKEND_NPU -> Backend.NPU
                else -> null
            }
        }

        @JvmStatic
        fun create(modelPath: String, backendType: Int, visionBackendType: Int, audioBackendType: Int, maxTokens: Int, useExternalCacheDir: Boolean) : EngineWrapper? {
            Log.i(TAG, "Creating engine wrapper.")

            val backend = toBackend(backendType)
            if (backend == null) {
                Log.e(TAG, "Expected valid backend type, got: $backendType")
                return null
            }

            val visionBackend = toBackend(visionBackendType)
            val audioBackend = toBackend(audioBackendType)

            return EngineWrapper(modelPath, backend, visionBackend, audioBackend, maxTokens, useExternalCacheDir)
        }
    }

    private val executor = Executors.newSingleThreadExecutor()
    private val engine: Engine

    init {
        val maxNumTokens = if (maxTokens == 0) null else maxTokens

        val context = UnityPlayer.currentContext
        val cacheDir = if (useExternalCacheDir && Environment.getExternalStorageState() == Environment.MEDIA_MOUNTED)
            context.externalCacheDir?.path ?: context.cacheDir?.path
        else
            context.cacheDir?.path

        val engineConfig = EngineConfig(modelPath, backend, visionBackend, audioBackend, maxNumTokens, cacheDir)
        engine = Engine(engineConfig)

        Log.i(TAG, "Initializing engine...")
        executor.submit {
            engine.initialize()
            Log.i(TAG, "Engine initialized!")
        }
    }

    fun isInitialized() : Boolean {
        return engine.isInitialized()
    }

    fun createConversation() : ConversationWrapper? {
        if (!checkInitForConvo()) return null
        Log.i(TAG, "Creating conversation wrapper.")

        val conversation = engine.createConversation()
        return ConversationWrapper(conversation)
    }

    fun createConversation(systemMessage: Message?, samplerConfig: SamplerConfig?) : ConversationWrapper? {
        if (!checkInitForConvo()) return null
        Log.i(TAG, "Creating conversation wrapper.")

        val config = ConversationConfig(systemMessage, samplerConfig = samplerConfig)
        val conversation = engine.createConversation(config)
        return ConversationWrapper(conversation)
    }

    private fun checkInitForConvo() : Boolean {
        if (!engine.isInitialized()) {
            Log.e(TAG, "Tried to create conversation while engine is initializing!")
            return false
        }

        return true
    }

    fun close() {
        if (engine.isInitialized()) {
            engine.close()
            Log.i(TAG, "Engine closed.")
        }
    }
}