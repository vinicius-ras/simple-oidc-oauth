/** Represents a claim, in its serializable format. */
type SerializableClaim = {
	/** The type of claim represented by this object. */
	type: string;
	/** The value of the claim. */
	value: string;
}

export default SerializableClaim;