#nullable enable
namespace Uralstech.UAI.LiteRTLM
{
    public enum LogLevel
    {
        Verbose     = 0,
        Debug       = 1,
        Info        = 2,
        Warning     = 3,
        Error       = 4,
        Fatal       = 5,
        Silent      = 1000,
    }
    
    /// <summary>Represents the type of sampler.</summary>
    public enum SamplerType : int
    {
        /// <summary>Probabilistically pick among the top k tokens.</summary>
        TopK    = 1,
                
        /// <summary>
        /// Probabilistically pick among the tokens such that the sum is greater
        /// than or equal to p tokens after first performing top-k sampling.
        /// </summary>
        TopP    = 2,

        /// <summary>Pick the token with maximum logit (i.e., argmax).</summary>
        Greedy  = 3,
    }

    /// <summary>Represents the type of input data.</summary>
    public enum InputDataType : int
    {
        /// <summary>A UTF-8 string.</summary>
        Text,
        Image,
        ImageEnd,
        Audio,
        AudioEnd,
    }
    
    public enum ActivationDataType
    {
        // <summary>Use float32 as the activation data type.</summary>
        Float32,

        // <summary>Use float16 as the activation data type.</summary>
        Float16,

        // <summary>Use int16 as the activation data type.</summary>
        Int16,

        // <summary>Use int8 as the activation data type.</summary>
        Int8,
    }
    
    /// <summary>Represents the type of a TokenUnion.</summary>
    public enum TokenUnionType
    {
        String  = 0,
        Ids     = 1,
    }
}