interface CheckboxProps {
  id: string;
  checked: boolean;
  onChange: (id: string, checked: boolean) => void;
  ariaLabel: string;
}

const Checkbox: React.FC<CheckboxProps> = ({
  id,
  checked,
  onChange,
  ariaLabel,
  ...props
}) => {
  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    onChange(id, event.target.checked);
  };
  return (
    <div className="govuk-checkboxes__item">
      <input
        className="govuk-checkboxes__input"
        type="checkbox"
        checked={checked}
        onChange={handleChange}
        aria-label={ariaLabel}
        {...props}
      />
      <label className="govuk-label govuk-checkboxes__label"></label>
    </div>
  );
};

export default Checkbox;
