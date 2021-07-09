import React from 'react';
import DatePicker, { ReactDatePickerProps } from 'react-datepicker';
import "react-datepicker/dist/react-datepicker.css";

/** Props for the {@link CustomDatePicker} functional component. */
export type CustomDatePickerProps = ReactDatePickerProps &
{
}


/** A wrapper around the DatePicker component (from the "react-datepicker" package) which allows for control over its styles. */
function CustomDatePicker(props: ReactDatePickerProps) {
	const {
		className,
		dateFormat="yyyy-MM-dd",
		...otherProps
	} = props
	return (
		<DatePicker className={`component-CustomDatePicker ${className}`} dateFormat={dateFormat} {...otherProps} />
	);
}

export default CustomDatePicker;