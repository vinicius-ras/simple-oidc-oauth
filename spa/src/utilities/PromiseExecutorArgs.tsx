/** Utility type which stores the arguments of a Promise executor.
 * The promise executor is the function passed when instantiating a new Promise (via new Promise(...executor...)).
 * @template T - The return type of the Promise. */
type PromiseExecutorArgs<T> = {
	/** Reference to the function used to resolve the promise with a specific result. */
	resolve: (result: T) => void;
	/** Reference to the function used to reject the promise, with an optional reason why. */
	reject: (reason?: any) => void;
}


export default PromiseExecutorArgs;