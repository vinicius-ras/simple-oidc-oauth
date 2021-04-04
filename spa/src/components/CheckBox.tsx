import { faCheck, faTimes } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import React from 'react';

/** Props for the {@link CheckBox} functional component. */
export type CheckBoxProps = JSX.IntrinsicElements["input"] &
{
	/** A flag indicating whether the checkbox is checked or not.
	 * This flag is mandatory for this component, which means that either the "onChange" or the "readOnly"
	 * props should be specified for client code that use this component. */
	checked: boolean;
	/** The text to be displayed as a label for the checkbox. */
	text?: string;
}


/** A simple component to wrap a checkbox and its label, while also allowing it
 * to be homogeneously stylized throughout the project. */
function CheckBox(props: CheckBoxProps) {
	const { text, className, checked, onChange, ...otherProps } = props;

	return (
		<label className={`component-CheckBox ${className ?? ""}`}>
			<input checked={checked} onChange={onChange} type="checkbox" {...otherProps} />
			<div className="component-CheckBox__checkmark">
				<FontAwesomeIcon icon={checked ? faCheck : faTimes} />
			</div>
			{
				text
				? <span className="ml-2">{text}</span>
				: null
			}
		</label>
	);
}

export default CheckBox;