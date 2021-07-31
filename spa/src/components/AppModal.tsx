import React from 'react';
import ReactModal from 'react-modal';

/** Props for the {@link AppModal} functional component. */
export type AppModalProps = Omit<ReactModal['props'], 'className'|'overlayClassName'>;


/** A modal used to display dialogs when the app requires the user's attention and/or input data.
 * This funcional component is just a wrapper to allow better control over styles for a React Modal ({@see ReactModal}) component. */
function AppModal(props: AppModalProps) {
	return (
		<ReactModal
			className={({
				base: "component-AppModal__content",
				afterOpen: "component-AppModal__content-open",
				beforeClose: "component-AppModal__content-closed",
			})}
			overlayClassName={({
				base: "component-AppModal__overlay",
				afterOpen: "component-AppModal__overlay-open",
				beforeClose: "component-AppModal__overlay-closed",
			})}
			{...props} />
	);
}

export default AppModal;